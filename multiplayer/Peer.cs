using System;
using Godot;

namespace LD56.multiplayer;

record Peer(Guid Id, int PeerId, WebRtcPeerConnection Connection);