using System;
using Godot;

namespace LD56;

public partial class Global : Node
{
    private const String DEFAULT_C2_BASE_URI = "wss://banana4.life";
    private String c2_base_uri;
    
    
    public static Global Instance { get; private set; }

    public State State;
    public PlayerManager PlayerManager = new();

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
        State = new ClientState(Multiplayer, c2_base_uri, playerName);

    }

    public void EnterServerState(String playerName)
    {
        State = new ServerState(Multiplayer, c2_base_uri);
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

}