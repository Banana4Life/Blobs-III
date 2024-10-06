using Godot;
using GodotPlugins.Game;

namespace LD56;

public partial class Global : Node
{
    private const string DEFAULT_C2_BASE_URI = "wss://banana4.life";
    private string c2_base_uri;

    private PackedScene worldScene = GD.Load<PackedScene>("res://world.tscn");
    private PackedScene mainMenuScene = GD.Load<PackedScene>("res://ui/main_menu/main_menu.tscn");
    private PackedScene countDownScene = GD.Load<PackedScene>("res://ui/transitions/countdown.tscn");

    public static Global Instance { get; private set; }

    public State State;
    public PlayerManager PlayerManager = new();
    private World world;

    public override void _Ready()
    {
        Instance = this;

        var config = new ConfigFile();
        var result = config.Load("user://config.cfg");
        c2_base_uri = DEFAULT_C2_BASE_URI;
        if (result == Error.Ok)
        {
            c2_base_uri = config.GetValue("c2server", "host", Variant.CreateFrom(DEFAULT_C2_BASE_URI)).AsString();
        }
    }

    public void EnterClientState(string playerName)
    {
        LoadCountdownScene();
        State = new ClientState(Multiplayer, c2_base_uri, playerName);
    }

    
    public void EnterServerState(string playerName)
    {
        State = new ServerState(Multiplayer, c2_base_uri, playerName);
        PlayerManager.AddPlayer(playerName, 1);
    }

    public override void _Process(double delta)
    {
        if (State != null)
        {
            State.Update(delta);
        }
    }

    public void SendPlayerInfo(string playerName)
    {
        RpcId(1, MethodName.ReceivePlayerInfo, playerName, Multiplayer.GetUniqueId());
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void ReceivePlayerInfo(string player, int peerId)
    {
        if (State is ServerState)
        {
            PlayerManager.AddPlayer(player, peerId);
        }
        else
        {
            GD.PrintErr("Got Player Info on Client?");
        }
    }

    public void LoadWorldScene(bool show)
    {
        GD.Print("Loading World Scene...");
        if (world == null)
        {
            world = worldScene.Instantiate<World>();
            GetTree().Root.AddChild(world);
        }

        if (show)
        {
            if (GetTree().CurrentScene is Countdown current1)
            {
                GD.Print($"Free {current1.Name} Scene...");
                current1.QueueFree();
            }
            else if (GetTree().CurrentScene is Mainmenu current2)
            {
                GD.Print($"Free {current2.Name} Scene...");
                current2.QueueFree();
            }
            GD.Print("Show World Scene...");
            GetTree().SetCurrentScene(world);
        }
        world.Visible = show;
        
    }
    
    private void LoadCountdownScene()
    {
        GetTree().ChangeSceneToPacked(countDownScene);
        LoadWorldScene(false);
    }

}