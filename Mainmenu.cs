using Godot;
using System;
using LD56;

record Peer(Guid Id, int PeerId, WebRtcPeerConnection Connection);

public partial class Mainmenu : Control
{
    [Export] public LineEdit playerName;

    public override void _Ready()
    {
        playerName.Text = NameGenerator.RandomName();
    }

    private void _on_join_button_pressed()
    {
        var name = UI_getPlayerName();
        if (Input.GetActionStrength("start-server") > 0.5)
        {
            Global.Instance.EnterServerState(name);
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