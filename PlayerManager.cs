using System;
using System.Collections.Generic;

namespace LD56;

public class PlayerManager
{
    public List<PlayerInfo> Players = new();

    public class PlayerInfo
    {
        public PlayerInfo(string name, long peerId, int size)
        {
            this.name = name;
            this.peerId = peerId;
            this.size = size;
        }


        public string name;
        public long peerId;
        public int size;
    }
}