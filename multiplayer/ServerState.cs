using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using LD56.multiplayer;

namespace LD56;

public class ServerState : State
{
    public enum NetworkState
    {
        CONNECTING,
        HOSTING,
    }

    private NetworkState state = NetworkState.CONNECTING;
    private readonly WebRtcMultiplayerPeer gamePeer = new();
    private readonly Guid myId = Guid.NewGuid();
    private readonly SignalingClient signalingClient;
    private List<Peer> serverClients = [];
    private readonly SimpleTimer hostingTimer = new(5f);
    private readonly MultiplayerApi multiplayer;
    private readonly string playerName;

    public ServerState(MultiplayerApi multiplayer, string baseUri, string playerName)
    {
        this.multiplayer = multiplayer;
        this.playerName = playerName;
        this.multiplayer.PeerConnected += OnPeerConnected;
        this.multiplayer.PeerDisconnected += OnPeerDisconnected;
        GD.Print($"Server connecting to C&C server: {baseUri}");
        signalingClient = new("server", baseUri, $"/ld56/signal/{myId}/host");
    }
    
    private void TransitionState(NetworkState newState)
    {
        GD.Print($"Server - Transition {state} -> {newState}");
        state = newState;
    }

    private void OnPeerConnected(long id)
    {
        GD.Print($"Server - {multiplayer.GetUniqueId()}: OnPeerConnected {id}");
    }

    private void OnPeerDisconnected(long id)
    {
        GD.Print($"Server - {multiplayer.GetUniqueId()}: OnPeerDisconnected {id}");
     
        var peer = serverClients.Find(it => it.PeerId == (int)id);
        if (peer != null)
        {
            peer.Connection.Close();
            serverClients.Remove(peer);
            
        }
        
        Global.Instance.PlayerManager.RemovePlayer(id);
        
        
    }

    public void Update(double dt)
    {
        signalingClient.Update();
        switch (state)
        {
            case NetworkState.CONNECTING:
                if (signalingClient.IsConnected)
                {
                    gamePeer.CreateServer();
                    multiplayer.MultiplayerPeer = gamePeer;
                    TransitionState(NetworkState.HOSTING);
                    Global.Instance.SetWindowTitle($"Server - {playerName}");
                    Global.Instance.SendPlayerInfo(playerName);
                    Global.Instance.SendPlayerReady();
                }
                break;
            case NetworkState.HOSTING:
                var packet = signalingClient.ReadPacket();
                if (packet != null)
                {
                    _handleServerHostingPacket(packet);
                }

                if (hostingTimer.Update(dt))
                {
                    signalingClient.HostingMessage(1 + serverClients.Count);
                }

                break;
        }
    }

    private int peerIdOffset = 10;

    public void _handleServerHostingPacket(Dictionary dict)
    {
        if (dict.ContainsKey("m"))
        {
            var m = dict["m"].AsString();
            var i = dict["i"].AsInt32();
            var name = dict["name"].AsString();
            var sourceId = new Guid(dict["sourceId"].AsString());

            var relevantPeer = serverClients.Find(it => it.Id == sourceId);
            if (relevantPeer != null)
            {
                relevantPeer.Connection.AddIceCandidate(m, i, name);
            }
            else
            {
                GD.PushWarning($"Received candidate for unknown peer: m={m} i={i} name={name} sourceId={sourceId}");
            }
        }
        else
        {
            var id = new Guid(dict["id"].AsString());
            if (dict.ContainsKey("offer"))
            {
                var offer = dict["offer"].AsString();
                var peer = serverClients.Find(it => it.Id == id);
                if (peer != null)
                {
                    peer.Connection.SessionDescriptionCreated += (type, sdp) =>
                    {
                        if (type == "answer")
                        {
                            signalingClient.AnsweringMessage(id, sdp);
                        }
                    };
                    peer.Connection.IceCandidateCreated += (media, index, name) =>
                    {
                        signalingClient.IceCandidateMessage(media, index, name, myId, id);
                    };
                    gamePeer.AddPeer(peer.Connection, peer.PeerId);
                    peer.Connection.SetRemoteDescription("offer", offer);
                }
            }
            else
            {
                // const int peerIdOffset = 10;
                int peerId = peerIdOffset++;
                // if (serverClients.Any())
                // {
                    // peerId = Math.Max(serverClients.Max(it => it.PeerId) + 1, peerIdOffset);
                // }
                // else
                // {
                    // peerId = peerIdOffset;
                // }

                var peer = new Peer(id, peerId, WebRtcUtil.NewConnection());
                GD.Print($"New Peer: {peer}");
                serverClients.Add(peer);
                signalingClient.HostAcceptsJoinMessage(id, peer.PeerId);
            }
        }
    }

    
}