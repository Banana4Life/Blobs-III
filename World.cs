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

    private RandomNumberGenerator random = new();

    public Player authorityPlayer;


    public override void _Ready()
    {
    }

    public int spawnRandomParticle()
    {
        var spawnRange = GetViewport().GetVisibleRect().Size;


        var particle = particlePrefab.Instantiate<Particle>();
        var spawnPos = new Vector2(
            random.RandfRange(-PlayArea.X / 2, PlayArea.X / 2),
            random.RandfRange(-PlayArea.Y / 2, PlayArea.Y / 2)
            // random.RandfRange(50f, 50f)
        );

        // TODO place particles outside of player range
        // TODO smarter location finding

        particle.GlobalPosition = spawnPos;
        particle.Name = "particle_" + i++;
        particle.RandomInit(random);

        AddChild(particle);
        return particle.size;
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

        var spawnPos = new Vector2(50, new Random().Next(50, 500));
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