using Godot;
using System;
using System.Collections.Generic;
using LD56;

public partial class World : Node2D
{
    [Export] public PackedScene particlePrefab;
    [Export] public PackedScene playerPrefab;

    private int players = 1;
    public int totalMass = 0;
    public int maxMass = 100000;
    public int i = 0;

    private RandomNumberGenerator random = new();


    public int spawnRandomParticle()
    {
        var spawnRange = GetViewport().GetVisibleRect().Size;
        

        var particle = particlePrefab.Instantiate();
        var spawnPos = new Vector2(
            random.RandfRange(0f, spawnRange.X),
            random.RandfRange(0f, spawnRange.Y)
            // random.RandfRange(50f, 50f)
        );

        // TODO place particles outside of player range
        // TODO respect other particles space?

        

        ((Node2D)particle).GlobalPosition = spawnPos;
        var sprite = particle.GetNode<Sprite2D>("Sprite2D");
        var randScale = random.RandfRange(3f, 10f);
        // var randScale = random.RandfRange(150f, 150f);
        ((Node2D)particle).Scale = new Vector2(randScale, randScale);

        var color = Color.FromHsv(random.RandfRange(0, 1f), 1f, 1f, random.RandfRange(0.2f, 0.4f));
        
        particle.Name = "particle_" + i++;
        ((Particle)particle).Color = color;
        ((Particle)particle).size = (int)(3 * (randScale * randScale) / 4);
        ((Particle)particle).seed = random.RandfRange(0f, 10000f);
        ((Particle)particle).mag = random.RandfRange(0.01f, 0.19f);
        ((Particle)particle).freq = random.RandfRange(0.5f, 5.5f);
            
        AddChild(particle);
        return ((Particle)particle).size;
    }

    public void spawnToMaxMass()
    {
        if (totalMass < maxMass)
        {
            totalMass += spawnRandomParticle();
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (Global.Instance.State is ServerState)
        {
            spawnToMaxMass();

            foreach (var playerInfo in Global.Instance.PlayerManager.Players)
            {
                if (!playerInfo.alive)
                {
                    SpawnPlayer(playerInfo);
                }
            }

            var playersToRemove = Global.Instance.PlayerManager.PlayersToRemove;
            while (playersToRemove.Count > 0)
            {
                var peerToRemove = playersToRemove.Dequeue();
                var found = GetNodeOrNull(peerToRemove.ToString());
                if (found != null)
                {
                    RemoveChild(found);
                }
            }

        }
    }
    
    private void SpawnPlayer(PlayerManager.PlayerInfo info)
    {
        GD.Print($"{Multiplayer.GetUniqueId()}: player spawn {info.name} {info.peerId}");

        var player = playerPrefab.Instantiate();
        player.Name = info.peerId.ToString();
        AddChild(player);
        
        var spawnPos = new Vector2(50, new Random().Next(50,500));
        RpcId(info.peerId, MethodName.initPlayerOnAuthority, info.name, info.peerId, spawnPos, info.size);
        info.alive = true;

    }
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void initPlayerOnAuthority(String displayName, long id, Vector2 position, int size)
    {
        var existing = GetNode<Player>(id.ToString());
        existing.GlobalPosition = position;
        existing.PlayerSize = size;
        existing.DisplayName = displayName;
        GD.Print($"{Multiplayer.GetUniqueId()}: Player {displayName}({id}) init size: {existing.PlayerSize} auth {existing.GetMultiplayerAuthority()}");
    }


    
}