using System.Collections.Generic;

namespace LD56;

public class PlayerManager
{
    public List<PlayerInfo> Players = new();

    public void AddPlayer(string name, long peerId)
    {
        Players.Add(new PlayerInfo(name, peerId, 50, false));
    }

    public class PlayerInfo
    {
        public PlayerInfo(string name, long peerId, int size, bool alive)
        {
            this.name = name;
            this.peerId = peerId;
            this.size = size;
            this.alive = alive;
        }


        public string name;
        public long peerId;
        public int size;
        public bool alive;
    }
}