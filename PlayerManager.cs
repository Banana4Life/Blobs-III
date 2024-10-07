using System.Collections.Generic;

namespace LD56;

public class PlayerManager
{
    public List<PlayerInfo> Players = new();
    public Queue<long> PlayersToRemove = new();

    public void AddPlayer(string name, long peerId)
    {
        Players.Add(new PlayerInfo(name, peerId, 0, false, false));
    }

    public void SetPlayerReady(long peerId)
    {
        var playerInfo = Players.Find(p => p.peerId == peerId);
        playerInfo.ready = true;
        playerInfo.alive = false;
    }

    public void RemovePlayer(long id)
    {
        PlayersToRemove.Enqueue(id);
    }

    public class PlayerInfo
    {
        public PlayerInfo(string name, long peerId, int size, bool alive, bool ready)
        {
            this.name = name;
            this.peerId = peerId;
            this.size = size;
            this.alive = alive;
            this.ready = ready;
        }


        public string name;
        public long peerId;
        public int size;
        public bool alive;
        public bool ready;
    }

    public void SetPlayerDead(long peerId)
    {
        var playerInfo = Players.Find(p => p.peerId == peerId);
        playerInfo.ready = false;
    }
}