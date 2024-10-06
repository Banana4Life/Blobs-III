using Godot;
using System;
using LD56;

record Peer(Guid Id, int PeerId, WebRtcPeerConnection Connection);

public partial class Mainmenu : Node2D
{
    private const String DEFAULT_C2_BASE_URI = "wss://banana4.life";
    private String c2_base_uri;

    [Export] public PackedScene player_scene;
    [Export] public PackedScene worldScene;

    private PlayerManager _manager = new();

    public override void _Ready()
    {
        LoadConfig();
        InitMainMenu();
        SetupMultiPlayer();
    }

    private void SetupMultiPlayer()
    {
        Multiplayer.PeerDisconnected += OnPlayerLeave;
        Multiplayer.ConnectedToServer += OnPlayerJoin;
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


    private void _on_host_button_pressed()
    {
        Global.Instance.State = new ServerState(Multiplayer, c2_base_uri);
        sendPlayerInfo(UI_getPlayerName() + "(Host)", 1);
        // GetTree().Root.AddChild(worldScene.Instantiate());
        // Visible = false;
    }

    private void _on_join_button_pressed()
    {
        Global.Instance.State = new ClientState(Multiplayer, c2_base_uri, UI_getPlayerName());
        // GetTree().Root.AddChild(worldScene.Instantiate());
        // Visible = false;
    }

    private string UI_getPlayerName()
    {
        return GetNode<LineEdit>("edName").Text;
    }

    private void OnPlayerLeave(long playerId)
    {
        // TODO check where the players needs to be removed
        // Server only?
        var found = GetNodeOrNull(playerId.ToString());
        if (found != null)
        {
            RemoveChild(found);
        }
    }

    private void OnPlayerJoin()
    {
        var peerId = Multiplayer.GetUniqueId();
        // Send RPC to Server that spawns the player for everyone
        RpcId(1, MethodName.sendPlayerInfo, UI_getPlayerName() ?? $"Player_{peerId}", peerId);
    }
}