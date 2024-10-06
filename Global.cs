using Godot;

namespace LD56;

public partial class Global : Node
{
    public static Global Instance { get; private set; }

    public State State;
    public PlayerManager PlayerManager = new();

    public override void _Ready()
    {
        Instance = this;
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

}