using System.Collections.Generic;
using System.Linq;
using Godot;

namespace LD56;

public partial class Global : Node
{
    public static readonly PackedScene toastScene = GD.Load<PackedScene>("res://ui/toast.tscn");
    private const string DEFAULT_C2_BASE_URI = "wss://banana4.life";
    private const string DEFAULT_C2_STATS_URI = "https://banana4.life/ld56/stats";
    private string c2_base_uri;

    private PackedScene worldScene = GD.Load<PackedScene>("res://world.tscn");
    private PackedScene mainMenuScene = GD.Load<PackedScene>("res://ui/main_menu/main_menu.tscn");
    private PackedScene countDownScene = GD.Load<PackedScene>("res://ui/transitions/countdown.tscn");
    private PackedScene respawnScene = GD.Load<PackedScene>("res://ui/transitions/respawn.tscn");
    public static Global Instance { get; private set; }

    public State State;
    public PlayerManager PlayerManager = new();
    public string StatsUri;
    private World world;
    private Respawn respawn;
    private Countdown countdown;
    private bool ready;

    public List<string> unlockedColors = ["PureGreen", "PureRed",  "PureBlue"];
    private ConfigFile config = new();

    public readonly RandomNumberGenerator Random = new();
    public string selectedColor;
    private Toast activeToast;


    public override void _Ready()
    {
        Instance = this;
        var result = config.Load("user://config.cfg");
        c2_base_uri = DEFAULT_C2_BASE_URI;
        StatsUri = DEFAULT_C2_STATS_URI;
        if (result == Error.Ok)
        {
            var cfg = config.GetValue("savegame", "colors", unlockedColors.ToArray());
            unlockedColors = cfg.AsStringArray().ToList();
            selectedColor = unlockedColors[Random.RandiRange(0, unlockedColors.Count)];
            GD.Print($"Unlocked Colors: {unlockedColors}");
            c2_base_uri = config.GetValue("c2server", "host", DEFAULT_C2_BASE_URI).AsString();
            StatsUri = config.GetValue("c2server", "stats", DEFAULT_C2_STATS_URI).AsString();
        }
    }

    public void EnterClientState(string playerName)
    {
        LoadCountdownScene();
        State = new ClientState(Multiplayer, c2_base_uri, playerName);
        SetWindowTitle("Joining");
    }


    public void EnterServerState(string playerName)
    {
        State = new ServerState(Multiplayer, c2_base_uri, playerName);
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
        GD.Print($"SendPlayerInfo: {playerName}");
        RpcId(1, MethodName.ReceivePlayerInfo, playerName, Multiplayer.GetUniqueId(), selectedColor);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void ReceivePlayerInfo(string playerName, int peerId, string selectedColor)
    {
        GD.Print($"ReceivePlayerInfo: {playerName} ({peerId})");
        if (State is ServerState)
        {
            PlayerManager.AddPlayer(playerName, peerId, selectedColor);
        }
        else
        {
            GD.PrintErr("Got Player Info on Client?");
        }
    }

    public void SendPlayerReady()
    {
        if (ready)
        {
            return;
        }

        ready = true;
        GD.Print($"SendPlayerReady: {Multiplayer.GetUniqueId()}");
        RpcId(1, MethodName.ReceivePlayerReady, Multiplayer.GetUniqueId());
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void ReceivePlayerReady(long peerId)
    {
        GD.Print($"ReceivePlayerReady: {peerId}");
        PlayerManager.SetPlayerReady(peerId);
    }

    public void SendPlayerDead()
    {
        ready = false;
        GD.Print($"SendPlayerDead: {Multiplayer.GetUniqueId()}");
        RpcId(1, MethodName.ReceivePlayerDead, Multiplayer.GetUniqueId());
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void ReceivePlayerDead(long peerId)
    {
        GD.Print($"ReceivePlayerReady: {peerId}");
        PlayerManager.SetPlayerDead(peerId);
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

    public void LoadRespawnScene(int score)
    {
        respawn = respawnScene.Instantiate<Respawn>();
        GetTree().Root.AddChild(respawn);
        GetTree().SetCurrentScene(respawn);

        respawn.GetNode<Label>("score").Text = score.ToString();
    }

    public void LoadCountdownSceneInWorld()
    {
        respawn.QueueFree();
        countdown = countDownScene.Instantiate<Countdown>();
        SetWindowTitle("Respawning");
        GetTree().Root.AddChild(countdown);
        GetTree().SetCurrentScene(countdown);
        // TODO render in screenspace
    }


    public void SetWindowTitle(string title)
    {
        var name = ProjectSettings.GetSetting("application/config/name", "Game").AsString();
        DisplayServer.WindowSetTitle($"{name} - {title}");
    }

    public void AwardUnlockedColor(string unlock)
    {
        if (!unlockedColors.Contains(unlock))
        {
            unlockedColors.Add(unlock);
            config.SetValue("savegame", "colors", unlockedColors.ToArray());
            config.Save("user://config.cfg");
            GD.Print($"Awarded {unlock}");
            Audio.Instance.Ding();
            if (activeToast != null)
            {
                activeToast.Hide();
                activeToast.QueueFree();
            }
            activeToast = toastScene.Instantiate<Toast>();
            AddChild(activeToast);
            activeToast.Present(unlock);
        }
    }
}