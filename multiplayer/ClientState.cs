using System;
using Godot;

namespace LD56;

public class ClientState : State
{
    public enum NetworkState
    {
        CONNECTING,
        JOINING,
        AWAIT_ACCEPT,
        OFFERING,
        CONNECTED,
    }

    private readonly WebRtcMultiplayerPeer gamePeer = new();
    private NetworkState state = NetworkState.CONNECTING;
    private readonly SignalingClient signalingClient;
    private readonly Guid myId = Guid.NewGuid();
    private int peerId;
    private Guid serverId;
    private WebRtcPeerConnection peerConnection;
    private readonly MultiplayerApi multiplayer;
    private readonly string playerName;
    private double timer;

    private void TransitionState(NetworkState newState)
    {
        GD.Print($"Client - Transition {state} -> {newState}");
        state = newState;
    }
    
    public ClientState(MultiplayerApi multiplayer, string baseUri, string playerName)
    {
        this.multiplayer = multiplayer;
        this.playerName = playerName;
        this.multiplayer.PeerConnected += OnPeerConnected;
        this.multiplayer.PeerDisconnected += OnPeerDisconnected;
        this.multiplayer.ConnectedToServer += OnConnectedToServer;
        this.multiplayer.ConnectionFailed += OnConnectionFailed;
        this.multiplayer.ServerDisconnected += OnServerDisconnected;
        GD.Print($"Client connecting to C&C server: {baseUri}");
        signalingClient = new("client", baseUri, $"/ld56/signal/{myId}/join");
        timer = 0;
    }

    private void OnPeerConnected(long id)
    {
        GD.Print($"Client - {multiplayer.GetUniqueId()}: OnPeerConnected {id}");
    }

    private void OnPeerDisconnected(long id)
    {
        GD.Print($"Client - {multiplayer.GetUniqueId()}: OnPeerDisconnected {id}");
    }

    private void OnConnectionFailed()
    {
        GD.Print($"Client - {multiplayer.GetUniqueId()}: OnConnectionFailed");
    }

    private void OnServerDisconnected()
    {
        GD.Print($"Client - {multiplayer.GetUniqueId()}: OnServerDisconnected");
    }

    private void OnConnectedToServer()
    {
        GD.Print($"Client - {multiplayer.GetUniqueId()}: connected to Server");
        TransitionState(NetworkState.CONNECTED);
        Global.Instance.SendPlayerInfo(playerName);
    }


    public void Update(double dt)
    {
        timer += dt;
        signalingClient.Update();
        switch (state)
        {
            case NetworkState.CONNECTING:
                UpdateConnectingState();
                break;
            case NetworkState.JOINING:
                UpdateJoiningState();
                break;
            case NetworkState.AWAIT_ACCEPT:
                UpdateAwaitAcceptState();
                break;
            case NetworkState.OFFERING:
                UpdateOfferingState();
                break;
            case NetworkState.CONNECTED:
                UpdateConnectedState(dt);
                break;
        }
    }

    private void UpdateConnectingState()
    {
        if (signalingClient.IsConnected)
        {
            TransitionState(NetworkState.JOINING);
        }
    }

    private void UpdateJoiningState()
    {
        signalingClient.JoinMessage();
        TransitionState(NetworkState.AWAIT_ACCEPT);
        timer = 0;
    }

    private void UpdateAwaitAcceptState()
    {
        if (WaitForCountdown()) return;
        var packet = signalingClient.ReadPacket();
        if (packet != null)
        {
            serverId = new Guid(packet["id"].AsString());
            peerId = packet["peerId"].AsInt32();
            TransitionState(NetworkState.OFFERING);

            peerConnection = WebRtcUtil.NewConnection();
            peerConnection.IceCandidateCreated += (media, index, name) =>
            {
                signalingClient.IceCandidateMessage(media, index, name, myId, serverId);
            };
            peerConnection.SessionDescriptionCreated += (type, sdp) =>
            {
                GD.Print($"Received SDP: {type}:\n{sdp}");
                signalingClient.OfferMessage(serverId, sdp);
            };
            gamePeer.CreateClient(peerId);
            multiplayer.MultiplayerPeer = gamePeer;
            GD.Print("UpdateAwaitAcceptState created client");


            gamePeer.AddPeer(peerConnection, 1);
            peerConnection.CreateDataChannel("test");
            peerConnection.CreateOffer();
        }
    }

    private void UpdateOfferingState()
    {
        if (WaitForCountdown()) return;
        if (peerConnection == null)
        {
            GD.PushWarning($"Updating in Offering state, but without a peer connection!");
            return;
        }

        // GD.Print($"PC state: {peerConnection.GetConnectionState()}");
        var packet = signalingClient.ReadPacket();
        if (packet != null)
        {
            if (packet.ContainsKey("m"))
            {
                var m = packet["m"].AsString();
                var i = packet["i"].AsInt32();
                var name = packet["name"].AsString();
                peerConnection.AddIceCandidate(m, i, name);
                signalingClient.IceCandidateMessage(m, i, name, myId, serverId);
            }
            else
            {
                var answer = packet["answer"].AsString();

                peerConnection.SetRemoteDescription("answer", answer);
            }
        }
    }

    private bool WaitForCountdown()
    {
        if (timer >= 2.5) // Waited for 3s to connect we host instead now...
        {
            GD.Print("No Server Found, Start Hosting...");
            Global.Instance.EnterServerState(playerName);
            return true;
        }

        return false;
    }

    private void UpdateConnectedState(double dt)
    {
       
    }
}