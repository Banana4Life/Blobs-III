using Godot;
using System;
using LD56;

record Peer(Guid Id, int PeerId, WebRtcPeerConnection Connection);

public partial class Mainmenu : Control
{
    [Export] public PackedScene player_scene;
    [Export] public PackedScene worldScene;
    [Export] public LineEdit playerName;


    public override void _Ready()
    {
        InitMainMenu();
    }

    private void InitMainMenu()
    {
        playerName.Text = NameGenerator.RandomName();
    }



    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void initPlayerOnAuthority(string displayName, long id, Vector2 position, int size)
    {
        var existing = GetNode<Player>(id.ToString());
        existing.GlobalPosition = position;
        existing.PlayerSize = size;
        existing.DisplayName = displayName;
        GD.Print($"{Multiplayer.GetUniqueId()}: Player {displayName}({id}) init size: {existing.PlayerSize} auth {existing.GetMultiplayerAuthority()}");
    }

    private void _on_host_button_pressed()
    {
        Global.Instance.EnterServerState(UI_getPlayerName());
        GetTree().Root.AddChild(worldScene.Instantiate());
        Visible = false;
    }

    private void _on_join_button_pressed()
    {
        Global.Instance.EnterClientState(UI_getPlayerName());
        GetTree().Root.AddChild(worldScene.Instantiate());
        Visible = false;
    }

    private string UI_getPlayerName()
    {
        return playerName.Text;
    }



}