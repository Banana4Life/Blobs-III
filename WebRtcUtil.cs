using Godot;
using Godot.Collections;

namespace LD56;

public static class WebRtcUtil
{
    public static WebRtcPeerConnection NewConnection()
    {
        var c = new WebRtcPeerConnection();
        Dictionary peerConfiguration = new()
        {
            {
                "iceServers",
                new Array {
                    new Dictionary
                    {
                        { "urls", new Array { "stun:stun.l.google.com:19302" } }
                    }
                }
            }
        };
        c.Initialize(peerConfiguration);
        return c;
    }
}