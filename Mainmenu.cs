using Godot;
using System;
using Godot.Collections;

public partial class Mainmenu : Node
{
    private const String DEFUALT_COMMAND_AND_CONTROL_SERVER = "banana4.life";
    private String command_and_control_server;
    const int GAME_PORT = 39875;
    NetworkState networkState;
    int playerCount = 1;
    ENetMultiplayerPeer gamePeer = new();
    PacketPeerUdp c2Peer;


    private String externalHost;
    private int externalPort;

    [Export] public PackedScene player_scene;
    [Export] public Timer timer;
    [Export] public TextEdit ipField;


    public override void _Ready()
    {
        var config = new ConfigFile();
        var result = config.Load("user://config.cfg");
        command_and_control_server = DEFUALT_COMMAND_AND_CONTROL_SERVER;
        if (result == Error.Ok)
        {
            command_and_control_server = config.GetValue("c2server", "host").AsString();
        }

        GD.Print($"Connecting to C&C server: {command_and_control_server}");
        c2Peer = new PacketPeerUdp();
        c2Peer.ConnectToHost(IP.ResolveHostname(command_and_control_server), GAME_PORT);
        
        Multiplayer.PeerDisconnected += OnPlayerDisconnected;
        Multiplayer.ConnectedToServer += _on_client_connected;
        Multiplayer.ConnectionFailed += _onFailed;
        Multiplayer.ServerDisconnected += OnServerDisconnected;
        Multiplayer.PeerConnected += _add_player;
    }
    
    private void OnPlayerDisconnected(long playerId)
    {
        GD.Print("OnPlayerDisconnected", playerId);
        var found = GetNodeOrNull($"Player {playerId}");
        if (found != null)
        {
            RemoveChild(found);
        }
    }

    private void OnServerDisconnected()
    {
        GD.Print("OnServerDisconnected");
    }
    
    private void _onFailed()
    {
        GD.Print("failed to connect to game server");
    } 

    public void _on_host_button_pressed()
    {
        // TODO dedicated server could go to hosting immediately
        GD.Print("hosting... discover external ip/port");
        networkState = NetworkState.DISCOVERY;
        timer.Start();
    }


    public void _add_player(long id = 1)
    {
        GD.Print($"Add player {id}");
        var name = $"Player {id}";
        var player = GetNodeOrNull(name);
        if (player == null)
        {
            player = player_scene.Instantiate();
        }

        player.Name = id.ToString();

        // ((Player)player).peerId = (int)id;
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
        Multiplayer.ConnectedToServer += _on_client_connected;
        gamePeer.CreateClient(ipField.Text, GAME_PORT);
        Multiplayer.MultiplayerPeer = gamePeer;
        networkState = NetworkState.CONNECTING;
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        switch (networkState)
        {
            case NetworkState.DISCOVERY:
                if (c2Peer.GetAvailablePacketCount() > 0)
                {
                    _handleDiscoveryResponsePacket();
                }
                break;
            case NetworkState.JOINING:
                if (c2Peer.GetAvailablePacketCount() > 0)
                {
                    _handleJoinResponsePacket();
                }

                break;
        }
    }

    // every 10s send info to c2
    public void On_timer_timeout()
    {
        switch (networkState)
        {
            case NetworkState.JOINING:
                c2Peer.PutPacket(_gatherC2Info(RequestType.JOIN).ToUtf8Buffer());
                break;
            case NetworkState.DISCOVERY:
                c2Peer.PutPacket(_gatherC2Info(RequestType.HOST).ToUtf8Buffer());
                break;
            case NetworkState.HOSTING:
                c2Peer.PutPacket(_gatherC2Info(RequestType.HOSTING).ToUtf8Buffer());
                break;
            case NetworkState.CONNECTED:
            case NetworkState.CONNECTING:
                break;
        }
    }

    public String _gatherC2Info(RequestType type)
    {
        var json = type switch
        {
            RequestType.HOST => $$"""
                                  {
                                      "type": "{{type}}"
                                  }
                                  """,
            RequestType.HOSTING => $$"""
                                  {
                                      "type": "{{type}}",
                                      "host": "{{externalHost}}",
                                      "port": {{externalPort}},
                                      "playerCount": {{playerCount}}
                                  }
                                  """,
            RequestType.JOIN => $$"""
                                    {
                                       "type": "{{type}}"
                                   }
                                  """,
            _ => "{}"
        };
        return json;
    }


    private void _on_client_connected()
    {
        GD.Print("Client connected");
        networkState = NetworkState.CONNECTED;
        // TODO start game        
    }

    public void _handleDiscoveryResponsePacket()
    {
        var dict = _parseResponseJson(c2Peer);
        if (dict.Count == 0)
        {
            return;
        }
        var host = dict["host"].AsString();
        var port = dict["port"].AsInt32();
        GD.Print($"Start Hosting on {host}:{port}...");
        externalHost = host;
        externalPort = port;
        
        var localPort = c2Peer.GetLocalPort();
        c2Peer.Close();
        gamePeer.CreateServer(localPort);
        
        Multiplayer.MultiplayerPeer = gamePeer;
        _add_player(); // host without joining
        networkState = NetworkState.HOSTING;
        timer.Stop();
        timer.Start();
    }

    public void _handleJoinResponsePacket()
    {
        var dict = _parseResponseJson(c2Peer);
        if (dict.Count == 0)
        {
            return;
        }

        var host = dict["host"].AsString();
        var port = dict["port"].AsInt32();
        var playerCount = dict["playerCount"].AsInt32();

        GD.Print($"Found Server {host}:{port} with {playerCount} players");
        var error = gamePeer.CreateClient(host, port);
        if (error != Error.Ok)
        {
            GD.Print("WTF?", error);
            return;
        }
        Multiplayer.MultiplayerPeer = gamePeer;
        networkState = NetworkState.CONNECTING;
    }

    private Dictionary _parseResponseJson(PacketPeerUdp peer)
    {
        var packet = peer.GetPacket();
        var jsonString = packet.GetStringFromUtf8();

        var json = Json.ParseString(jsonString);

        if (json.VariantType == Variant.Type.Nil) return new Dictionary();

        return json.AsGodotDictionary();
    }


    public enum RequestType
    {
        JOIN,
        HOST, // i want to host, what is my external ip/port?
        HOSTING // i am hosting on ip/port
    }


    public enum NetworkState
    {
        // server states
        DISCOVERY,
        HOSTING,
        // client states
        JOINING,
        CONNECTING,
        CONNECTED
    }
}