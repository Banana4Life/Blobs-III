using Godot;
using System;
using LD56;

public partial class World : Node2D
{
    [Export] public PackedScene particlePrefab;
    [Export] public PackedScene playerPrefab;

    private int players = 1;
    public int totalMass = 0;
    [Export] public int maxMass;
    [Export] public Vector2 PlayArea;
    public int i = 0;

    private RandomNumberGenerator random;

    public Player authorityPlayer;


    public override void _Ready()
    {
        random = Global.Instance.Random;
    }

    public int spawnRandomParticle()
    {
        var spawnRange = GetViewport().GetVisibleRect().Size;


        var particle = particlePrefab.Instantiate<Particle>();
        var spawnPos = randomSpawnPos();

        // TODO place particles outside of player range
        // TODO smarter location finding

        particle.GlobalPosition = spawnPos;
        particle.Name = "particle_" + i++;
        particle.RandomInit(random);

        AddChild(particle);
        return particle.size;
    }

    private Vector2 randomSpawnPos()
    {
        var spawnPos = new Vector2(
            random.RandfRange(-PlayArea.X / 2, PlayArea.X / 2),
            random.RandfRange(-PlayArea.Y / 2, PlayArea.Y / 2)
            // random.RandfRange(50f, 50f)
        );
        return spawnPos;
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
        if (authorityPlayer != null)
        {
            var cam = GetNode<Camera2D>("Camera2D");
            cam.Position = cam.Position.Lerp(authorityPlayer.Position, 0.1f);

            const float stepSize = 200;
            const float minStep = 10;
            
            var scaledSize = Math.Min(Mathf.Floor(authorityPlayer.PlayerSize / stepSize), minStep);
            cam.Zoom = cam.Zoom.Lerp(Vector2.One / (Vector2.One * (float)Math.Log10(scaledSize + 1d)), 0.05f);
        }

        if (Global.Instance.State is ServerState)
        {
            spawnToMaxMass();

            foreach (var playerInfo in Global.Instance.PlayerManager.Players)
            {
                if (!playerInfo.alive && playerInfo.ready)
                {
                    SpawnPlayer(playerInfo);
                }

                if (playerInfo.alive && !playerInfo.ready)
                {
                    var deadPlayer =  GetNodeOrNull<Player>(playerInfo.peerId.ToString());
                    if (deadPlayer != null)
                    {
                        GD.Print($"Player {playerInfo.name} is dead and got removed");
                        deadPlayer.QueueFree();
                    }
                }
            }

            var playersToRemove = Global.Instance.PlayerManager.PlayersToRemove;
            while (playersToRemove.Count > 0)
            {
                var peerToRemove = playersToRemove.Dequeue();
                GD.Print($"Removing Player {peerToRemove}");
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

        var player = playerPrefab.Instantiate<Player>();
        player.Name = info.peerId.ToString();
        player.DisplayName = info.name;
        player.GrowPlayer();
        AddChild(player);

        var spawnPos = randomSpawnPos();
        RpcId(info.peerId, MethodName.initPlayerOnAuthority, info.name, info.peerId, spawnPos, player.PlayerSize);
        info.alive = true;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void initPlayerOnAuthority(string displayName, long id, Vector2 position, int size)
    {
        var existing = GetNode<Player>(id.ToString());
        existing.GlobalPosition = position;
        existing.PlayerSize = size;
        existing.DisplayName = displayName;
        GD.Print($"{Multiplayer.GetUniqueId()}: Player {displayName}({id}) init size: {existing.PlayerSize} auth {existing.GetMultiplayerAuthority()}");
        

        authorityPlayer = existing;
        var cam = GetNode<Camera2D>("Camera2D");
        cam.Position = authorityPlayer.Position;
    }
}