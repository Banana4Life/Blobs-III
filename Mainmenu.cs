using Godot;
using System;

public partial class Mainmenu : Node
{
    private const String COMMAND_AND_CONTROL_SERVER = "banana4.life";
    const int GAME_PORT = 39875;
    NetworkState networkState;
    int playerCount = 1;
    ENetMultiplayerPeer gamePeer = new();
    PacketPeerUdp c2Peer = initC2Connection();


    [Export] 
    public PackedScene player_scene;
    [Export] 
    public Timer timer;
    [Export] 
    public TextEdit ipField;


    public static PacketPeerUdp initC2Connection()
    {
        var peer = new PacketPeerUdp();
        peer.ConnectToHost(IP.ResolveHostname(COMMAND_AND_CONTROL_SERVER), GAME_PORT); // TODO resolve ip
        return peer;
    }


    public void _on_host_button_pressed()
    {
        GD.Print("hosting...");
        gamePeer.CreateServer(GAME_PORT);
        Multiplayer.MultiplayerPeer = gamePeer;
        Multiplayer.PeerConnected += _add_player;
        _add_player();
        networkState = NetworkState.HOSTING;
        // TODO clear incoming packets
        timer.Start();
    }


    public void _add_player(long id = 1)
    {
        GD.Print("Add player");
        var player = player_scene.Instantiate();
        player.Name = id.ToString();
        CallDeferred("add_child", player);
    }


// joining a random server decided by c2
    public void _on_join_button_pressed()
    {
        networkState = NetworkState.JOINING;
        // TODO clear incoming packets
        timer.Start();
    }

    public void _on_join_ip_pressed()
    {
        gamePeer.CreateClient(ipField.Text, GAME_PORT);
        Multiplayer.MultiplayerPeer = gamePeer;
        Multiplayer.Connect("connected_to_server", Callable.From(_on_client_connected));
        networkState = NetworkState.CONNECTING;
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        switch (networkState)
        {
            case NetworkState.JOINING:
                if (c2Peer.GetAvailablePacketCount() > 0)
                {
                    _handleJoinPacket();
                }

                break;
        }
    }

    public void On_timer_timeout()
    {
    }

    public String _gatherC2Info(RequestType type)
    {
        var json = type switch
        {
            RequestType.JOIN => $$"""
                                  {
                                      "type": {{type}}
                                      "playerCount": {{playerCount}}
                                  }
                                  """,
            RequestType.HOST => $$"""
                                    {
                                       "type":  {{type}}
                                   }
                                  """,
            _ => "{}"
        };
        return Json.Stringify(json);
    }


    public void _on_client_connected()
    {
        networkState = NetworkState.CONNECTED;
// TODO start game        
    }


    public void _handleJoinPacket()
    {
        var packet = c2Peer.GetPacket();
        var jsonString = packet.GetStringFromUtf8();

        var json = Json.ParseString(jsonString);
        
        if (json.VariantType == Variant.Type.Nil) return;

        var dict = json.AsGodotDictionary();
        
        var host = dict["host"].AsString();
        var port = dict["count"].AsInt32();
        var players = dict["players"].AsInt32();
        
        GD.Print($"Found Server {host}:{port} with {players} players");
        gamePeer.CreateClient(host, port);
        Multiplayer.MultiplayerPeer = gamePeer;
        Multiplayer.Connect("connected_to_server", Callable.From(_on_client_connected));
        networkState = NetworkState.CONNECTING;
    }


    // every 10s send info to c2
    public void _on_timer_timeout()
    {
        switch (networkState)
        {
            case NetworkState.JOINING:
                c2Peer.PutPacket(_gatherC2Info(RequestType.JOIN).ToUtf8Buffer());
                break;
            case NetworkState.HOSTING:
                c2Peer.PutPacket(_gatherC2Info(RequestType.HOST).ToUtf8Buffer());
                break;
            case NetworkState.CONNECTED:
            case NetworkState.CONNECTING:
                break;
        }
    }


    public enum RequestType
    {
        JOIN,
        HOST
    }


    public enum NetworkState
    {
        HOSTING,
        JOINING,
        CONNECTING,
        CONNECTED
    }
}