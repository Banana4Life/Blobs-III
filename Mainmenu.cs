using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;
using LD56;

public partial class Mainmenu : Node
{
    List<String> names = new()
    {
        "Bob",
        "Alice",
        "Charlie",
        "Eve",
        "Dave",
        "Grace",
        "Heidi",
        "Ivan",
        "Judy",
        "Mallory"
    };


    private const String DEFUALT_COMMAND_AND_CONTROL_SERVER = "banana4.life";
    private String command_and_control_server;
    const int GAME_PORT = 39875;
    NetworkState networkState;
    int playerCount = 1;
    ENetMultiplayerPeer gamePeer = new();
    PacketPeerUdp c2Peer;


    private String externalHost = "127.0.0.1";
    private int externalPort = 39875;

    [Export] public PackedScene player_scene;
    [Export] public Timer timer;
    [Export] public TextEdit ipField;

    private PlayerManager _manager = new();

    public override void _Ready()
    {
        var config = new ConfigFile();
        var result = config.Load("user://config.cfg");
        command_and_control_server = DEFUALT_COMMAND_AND_CONTROL_SERVER;
        if (result == Error.Ok)
        {
            command_and_control_server = config.GetValue("c2server", "host", Variant.CreateFrom(DEFUALT_COMMAND_AND_CONTROL_SERVER)).AsString();
        }

        GD.Print($"Connecting to C&C server: {command_and_control_server}");
        c2Peer = new PacketPeerUdp();
        c2Peer.ConnectToHost(IP.ResolveHostname(command_and_control_server), GAME_PORT);

        Multiplayer.PeerConnected += OnPlayerConnected;
        Multiplayer.PeerDisconnected += OnPlayerDisconnected;
        Multiplayer.ConnectedToServer += OnConnectedToServer;
        Multiplayer.ConnectionFailed += OnConnectionFail;
        Multiplayer.ServerDisconnected += OnServerDisconnected;
        GetNode<LineEdit>("edName").Text = names[new Random().Next(0, names.Count)];
    }

    private void OnPlayerConnected(long playerId)
    {
        GD.Print(Multiplayer.GetUniqueId(), ": OnPlayerConnected ", playerId);
    }

    private void OnPlayerDisconnected(long playerId)
    {
        GD.Print("OnPlayerDisconnected", playerId);
        var found = GetNodeOrNull(playerId.ToString());
        if (found != null)
        {
            RemoveChild(found);
        }
    }

    private void OnServerDisconnected()
    {
        GD.Print("OnServerDisconnected");
    }

    private void OnConnectionFail()
    {
        GD.Print("failed to connect to game server");
    }

    public void _on_host_button_2_pressed()
    {
        GD.Print($"hosting... {externalHost}");
        networkState = NetworkState.HOSTING;
        timer.Start();
        gamePeer.CreateServer(GAME_PORT);
        // TODO compression?
        var playerName = GetNode<LineEdit>("edName").Text;
        sendPlayerInfo(playerName + "(Host)", 1);
        Multiplayer.MultiplayerPeer = gamePeer;
    }

    public void _on_host_button_pressed()
    {
        // TODO dedicated server could go to hosting immediately
        GD.Print("hosting... discover external ip/port");
        networkState = NetworkState.DISCOVERY;
        timer.Start();
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void sendPlayerInfo(String name, long id)
    {
        // TODO add player to game manager
        GD.Print($"{Multiplayer.GetUniqueId()}: send player info {name}({id})");
        if (Multiplayer.IsServer())
        {
            
            var spawnPos = new Vector2(50, new Random().Next(50,500));
            var spawnSize = new Random().Next(50, 150);

            GD.Print($"{Multiplayer.GetUniqueId()}: broadcasting spawn {name}({id}) to {spawnPos} size {spawnSize}...");
            _manager.Players.Add(new PlayerManager.PlayerInfo(name, id, spawnSize));
            
            SpawnPlayer(name, id);
            if (id != 1)
            {
                RpcId(id, MethodName.initPlayerOnAuthority, name, id, spawnPos, spawnSize);
            }
            else
            {
                initPlayerOnAuthority(name, id, spawnPos, spawnSize);
            }
        }
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void initPlayerOnAuthority(String displayName, long id, Vector2 position, int size)
    {
        var existing = GetNode<Player>(id.ToString());
        existing.GlobalPosition = position;
        existing.PlayerSize = size;
        existing.DisplayName = displayName;
        GD.Print($"{Multiplayer.GetUniqueId()}: Player {displayName}({id}) init size: {existing.PlayerSize} auth {existing.GetMultiplayerAuthority()}");
    }
    
    private void SpawnPlayer(string displayName, long id)
    {
        GD.Print($"{Multiplayer.GetUniqueId()}: player spawn {displayName} {id}");

        var player = player_scene.Instantiate();
        player.Name = id.ToString();
        AddChild(player);
    }

    // joining a random server decided by c2
    public void _on_join_button_pressed()
    {
        networkState = NetworkState.JOINING;
        GD.Print($"Finding Server to Join...");
        // TODO clear incoming packets
        timer.Start();
    }

    public void _on_join_ip_pressed()
    {
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
                    _handleHostDiscoveryResponsePacket();
                }

                break;
            case NetworkState.JOINING:
                if (c2Peer.GetAvailablePacketCount() > 0)
                {
                    _handleClientJoinResponsePacket();
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


    private void OnConnectedToServer()
    {
        var peerId = Multiplayer.GetUniqueId();
        GD.Print($"{peerId}: Client connected to Server");
        networkState = NetworkState.CONNECTED;

        var playerName = GetNode<LineEdit>("edName").Text;
        RpcId(1, MethodName.sendPlayerInfo, playerName ?? $"Player_{peerId}", peerId);

        // Rpc(MethodName.PlayerJoin, peerId);
        // TODO start game        
    }


    public void _handleHostDiscoveryResponsePacket()
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
        // TODO compression?

        Multiplayer.MultiplayerPeer = gamePeer;
        var playerName = GetNode<LineEdit>("edName").Text;
        sendPlayerInfo(playerName + "(Host)", 1);
        networkState = NetworkState.HOSTING;
        timer.Stop();
        timer.Start();
    }

    public void _handleClientJoinResponsePacket()
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