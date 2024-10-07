using System;
using Godot;
using LD56;

public partial class Mainmenu : Control
{
    [Export] public LineEdit playerName;
    [Export] public Label serverCount;
    [Export] public Label playerCount;
    private SimpleTimer _simpleTimer;

    public override void _Ready()
    {
        DisplayServer.WindowSetTitle($"LD56 - Main Menu");
        playerName.Text = NameGenerator.RandomName();
        serverCount.Text = 1.ToString();
        playerCount.Text = 1.ToString();

        _simpleTimer = new SimpleTimer(15);
    }

    public override void _Process(double delta)
    {
        if (_simpleTimer.Update(delta))
        {
            UpdateStats();
        }
    }

    private void _on_random_name_button_pressed()
    {
        playerName.Text = NameGenerator.RandomName();
    }

    private void _on_join_button_pressed()
    {
        var name = UI_getPlayerName();
        if (Input.GetActionStrength("start-server") > 0.5)
        {
            Global.Instance.EnterServerState(name);
            Global.Instance.LoadWorldScene(true);
        }
        else
        {
            Global.Instance.EnterClientState(name);
        }
    }

    private string UI_getPlayerName()
    {
        return playerName.Text;
    }

    private void UpdateStats()
    {
        var statsRequest = new HttpRequest();
        AddChild(statsRequest);
        statsRequest.RequestCompleted += StatsRequestOnRequestCompleted;
        statsRequest.RequestCompleted += (_, _, _, _) => RemoveChild(statsRequest);
        statsRequest.Request(Global.Instance.StatsUri);
    }

    private void StatsRequestOnRequestCompleted(long result, long responsecode, string[] headers, byte[] body)
    {
        GD.Print("Received");
        var variant = Json.ParseString(body.GetStringFromUtf8());
        if (variant.VariantType == Variant.Type.Nil)
        {
            GD.Print("json not found");
            return;
        }
        
        GD.Print("set values");
        var statsDictionary = variant.AsGodotDictionary();
        serverCount.Text = Math.Max(statsDictionary["servers"].AsInt32(), 1 ).ToString();
        playerCount.Text = Math.Max(statsDictionary["players"].AsInt32(), 1 ).ToString();
        GD.Print("values set");
    }
}