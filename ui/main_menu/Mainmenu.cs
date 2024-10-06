using Godot;
using LD56;

public partial class Mainmenu : Control
{
    [Export] public LineEdit playerName;

    public override void _Ready()
    {
        DisplayServer.WindowSetTitle($"LD56 - Main Menu");
        playerName.Text = NameGenerator.RandomName();
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

}