using Godot;
using System;
using LD56;

record Peer(Guid Id, int PeerId, WebRtcPeerConnection Connection);

public partial class Mainmenu : Node
{
    private const String DEFAULT_C2_BASE_URI = "wss://banana4.life";
    private String c2_base_uri;
    const int GAME_PORT = 39875;
    private State state;

    private String externalHost = "127.0.0.1";
    private int externalPort = GAME_PORT;

    [Export] public PackedScene player_scene;
    [Export] public TextEdit ipField;

    private PlayerManager _manager = new();

    public override void _Ready()
    {
        LoadConfig();
        InitMainMenu();
        SetupMultiPlayer();
    }

    private void SetupMultiPlayer()
    {
        Multiplayer.PeerConnected += OnPlayerConnected;
        Multiplayer.PeerDisconnected += OnPlayerDisconnected;
        Multiplayer.ConnectedToServer += OnConnectedToServer;
        Multiplayer.ConnectionFailed += OnConnectionFail;
        Multiplayer.ServerDisconnected += OnServerDisconnected;
    }

    private void InitMainMenu()
    {
        GetNode<LineEdit>("edName").Text = NameGenerator.RandomName();
    }

    private void LoadConfig()
    {
        var config = new ConfigFile();
        var result = config.Load("user://config.cfg");
        c2_base_uri = DEFAULT_C2_BASE_URI;
        if (result == Error.Ok)
        {
            c2_base_uri = config.GetValue("c2server", "host", Variant.CreateFrom(DEFAULT_C2_BASE_URI)).AsString();
        }
    }

    private void OnPlayerConnected(long playerId)
    {
        GD.Print(Multiplayer.GetUniqueId(), ": OnPlayerConnected ", playerId);
    }

    private void OnPlayerDisconnected(long playerId)
    {
        GD.Print("OnPlayerDisconnected", playerId);
        if (state != null)
        {
            state.PlayerDisconnected((int)playerId);
        }
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


    public void _on_host_button_pressed()
    {
        // TODO dedicated server could go to hosting immediately
        GD.Print("hosting... discover external ip/port");
        state = new ServerState(Multiplayer, c2_base_uri);
        var playerName = GetNode<LineEdit>("edName").Text;
        sendPlayerInfo(playerName + "(Host)", 1);
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
        state = new ClientState(Multiplayer, c2_base_uri);
    }



    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (state != null)
        {
            state.Update(delta);
        }
    }

    private void OnConnectedToServer()
    {
        var peerId = Multiplayer.GetUniqueId();
        if (state is ClientState clientState)
        {
            clientState.ConnectedToServer();
        }

        var playerName = GetNode<LineEdit>("edName").Text;
        RpcId(1, MethodName.sendPlayerInfo, playerName ?? $"Player_{peerId}", peerId);

        // Rpc(MethodName.PlayerJoin, peerId);
        // TODO start game        
    }
}