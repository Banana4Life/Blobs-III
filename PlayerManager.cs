using System.Collections.Generic;

namespace LD56;

public class PlayerManager
{
    public List<PlayerInfo> Players = new();
    public Queue<long> PlayersToRemove = new();

    public void AddPlayer(string name, long peerId, string selectedColor)
    {
        Players.Add(new PlayerInfo(name, peerId, 0, false, false, selectedColor));
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
        public PlayerInfo(string name, long peerId, int size, bool alive, bool ready, string selectedColor)
        {
            this.name = name;
            this.peerId = peerId;
            this.size = size;
            this.alive = alive;
            this.ready = ready;
            this.selectedColor = selectedColor;
        }


        public string name;
        public long peerId;
        public int size;
        public bool alive;
        public bool ready;
        public string selectedColor;
    }

    public void SetPlayerDead(long peerId)
    {
        var playerInfo = Players.Find(p => p.peerId == peerId);
        playerInfo.ready = false;
    }
}