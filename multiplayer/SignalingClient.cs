using System;
using Godot;
using Godot.Collections;

namespace LD56;

public class SignalingClient
{
    private readonly string role;
    private const string DEFAULT_C2_BASE_URI = "wss://banana4.life";
    
    private WebSocketPeer _webSocket;
    private readonly string uri;
    
    public bool IsConnected => _webSocket.GetReadyState() == WebSocketPeer.State.Open;

    public SignalingClient(string role, string baseUri, string path)
    {
        this.role = role;
        uri = $"{baseUri ?? DEFAULT_C2_BASE_URI}{path}";
    }

    public void Update()
    {
        if (_webSocket == null)
        {
            _webSocket = new WebSocketPeer();
            GD.Print($"Connecting to: {uri}");
            var result = _webSocket.ConnectToUrl(uri);
            if (result != Error.Ok)
            {
                GD.PushError($"Failed to connect to C2 at {uri}: {result}");
                _webSocket = null;
                return;
            }
        }
        if (_webSocket.GetReadyState() != WebSocketPeer.State.Closed)
        {
            _webSocket.Poll();
        }
    }

    private void SendText(string json)
    {
        GD.Print($"{role} - Send packet: {json}");
        _webSocket.SendText(json);
    }

    private void SendText(Dictionary json)
    {
        SendText(Json.Stringify(json));
    }

    public Dictionary ReadPacket()
    {
        if (_webSocket.GetAvailablePacketCount() <= 0)
        {
            return null;
        }
        var packet = _webSocket.GetPacket();
        var jsonString = packet.GetStringFromUtf8();

        GD.Print($"{role} - Received packet: {jsonString}");
        var json = Json.ParseString(jsonString);

        if (json.VariantType == Variant.Type.Nil)
        {
            return null;
        }

        return json.AsGodotDictionary();
    }

    public void HostingMessage(int playerCount)
    {
        var dict = new Dictionary
        {
            {"_type", "controllers.HostingMessage"},
            {"playerCount", playerCount},
        };
        SendText(dict);
    }

    public void JoinMessage()
    {
        var dict = new Dictionary
        {
            {"_type", "controllers.JoinMessage"},
        };
        SendText(dict);
    }

    public void IceCandidateMessage(string media, long index, string name, Guid source, Guid destination)
    {
        var dict = new Dictionary
        {
            { "_type", "controllers.IceCandidateMessage" },
            { "m", media },
            { "i", index },
            { "name", name },
            { "sourceId", source.ToString() },
            { "destinationId", destination.ToString() },
        };
        SendText(dict);
    }

    public void HostAcceptsJoinMessage(Guid id, int peerId)
    {
        var dict = new Dictionary
        {
            { "_type", "controllers.HostAcceptsJoinMessage" },
            { "id", id.ToString() },
            { "peerId", peerId },
        };
        SendText(dict);
    }

    public void OfferMessage(Guid destination, string offer)
    {
        var dict = new Dictionary
        {
            {"_type", "controllers.OfferMessage"},
            {"destination", destination.ToString() },
            {"offer", offer},
        };
        SendText(dict);
    }

    public void AnsweringMessage(Guid destination, string answer)
    {
        var dict = new Dictionary
        {
            {"_type", "controllers.AnsweringMessage"},
            {"destination", destination.ToString()},
            {"answer", answer},
        };
        SendText(dict);
    }
}