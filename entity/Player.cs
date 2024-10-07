using System;
using System.Linq;
using Godot;
using LD56;

public partial class Player : CharacterBody2D, MassContributor
{
    public static readonly PackedScene eatParticles = GD.Load<PackedScene>("res://particles/eat_particles.tscn");

    
    public const float SPEED = 300.0f;

    public bool aiControlled;
    
    [Export] public string DisplayName;
    [Export] public int PlayerSize;
    [Export] public int score;
    [Export] public Color PlayerColor;
    public Vector2 targetScale;

    public int ContributedMass => PlayerSize;
    public double eatenCd;
    private double starving = 5;
    
    public override void _Ready()
    {
        GetNode<Node2D>("scaled").Scale = Vector2.Zero;
        // var syncher = GetNode<MultiplayerSynchronizer>("PlayerSync");
        // syncher.SetVisibilityFor(0, false);
        (GetNode<Sprite2D>("scaled/Sprite2D").Material as ShaderMaterial)
            .SetShaderParameter("bodyColor", PlayerColor);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Multiplayer.IsServer() && aiControlled)
        {
            var massContributors = GetTree().GetNodesInGroup("MassContributor");
            var players = massContributors.Where(mc => mc is Player { aiControlled: false });
            var max = !players.Any() ? 0 : players.Max(mc => ((Player)mc).PlayerSize);
            max = Math.Max(max, 450);
            if (PlayerSize > max)
            {
                // TODO do stuff when player size was reached
                return;
            }
            var sorted = massContributors.Where(mc =>
            {
                if (mc is Particle pa)
                {
                    return pa.size < PlayerSize;
                }

                if (mc is Player pl)
                {
                    return pl.PlayerSize < PlayerSize;
                }

                return false;
            }).OrderBy(mc => (((Node2D)mc).GlobalPosition - GlobalPosition).LengthSquared());
            if (sorted.Count() == 0)
            {
                if (starving < 0)
                {
                    GD.Print($"{Name} cannot find any prey");
                    PlayerDied();    
                }
                return;
            }
            Velocity = (((Node2D)sorted.First()).GlobalPosition - GlobalPosition).Normalized() * Speed();
            MoveAndSlide();
            detectCollision(delta);
            return;
        }

        // GrowPlayer(2);
        if (IsMultiplayerAuthority())
        {
            Velocity = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down") *  Speed();
            MoveAndSlide();
            detectCollision(delta);
        }
    }

    private float Speed()
    {
        return Math.Max(150, SPEED - PlayerSize / 100f);
    }

    public void SpawnColoredParticlesOnScaled(Node2D on, PackedScene particleSystemScene, Vector2 at, Color color)
    {
        var particle = particleSystemScene.Instantiate<GpuParticles2D>();
        if (particle.ProcessMaterial is ParticleProcessMaterial particleProcessMaterial)
        {
            particleProcessMaterial.SetColor(color);

            particle.Emitting = true;
            on.GetNode<Node2D>("scaled").AddChild(particle);
            particle.GlobalPosition = at;
        }
    }
    
    private void detectCollision(double delta)
    {
        var world = GetParent<World>();
        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            var collision = GetSlideCollision(i);
            // GD.Print($"{DisplayName} collided {collision.GetCollider()}");

            if (collision.GetCollider() is RigidBody2D rb)
            {
                if (rb is not Particle particle || particle.size >= PlayerSize)
                {
                    rb.ApplyCentralImpulse(-collision.GetNormal() * 6);
                }
                else
                {
                    rb.ApplyCentralImpulse(-collision.GetNormal() * 3);
                }
              
            }
            if (collision.GetCollider() is Particle pa)
            {
                if (pa.size < PlayerSize)
                {
                    if (pa.eatenCd < 0)
                    {
                        pa.eatenCd = 0.1;
                        
                        // var massEaten = (int) (Mathf.Max(5, pa.size * delta * 25));
                        var eatRate = 2;
                        var massEaten = (int)Mathf.Min(PlayerSize * delta * eatRate, pa.size);
                        GrowPlayer(Mathf.Max(1, massEaten / 2));
                        RpcId(1, MethodName.EatParticle, pa.Name, massEaten);
                        SpawnColoredParticlesOnScaled(pa, eatParticles, collision.GetPosition(), pa.Color);
                    }
                        
                }
            }

            if (collision.GetCollider() is Player pl)
            {
                if (PlayerSize > pl.PlayerSize)
                {
                    if (pl.eatenCd < 0)
                    {
                        var massEaten = (int) (Mathf.Max(5, pl.PlayerSize * delta * 25));
                        GrowPlayer(Mathf.Max(1, massEaten / 4));
                        // GD.Print($"{Multiplayer.GetUniqueId()} : {DisplayName} eats {massEaten} of {pl.DisplayName}");
                            
                        RpcId(pl.authorityFromName(), MethodName.EatPlayer, pl.Name, massEaten);
                        pl.eatenCd = 0.1;
                        
                        SpawnColoredParticlesOnScaled(pl, eatParticles, collision.GetPosition(), pl.PlayerColor);
                    }
                }
            }
        }
    }

    public int authorityFromName()
    {
        if (Name.ToString().StartsWith("AI"))
        {
            return 1;
        }

        return int.Parse(Name);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void EatParticle(string name, int mass)
    {
        var particle = GetParent().GetNode<Particle>(name);
        if (particle != null)
        {
            particle.Shrink(mass);
        }
    }

    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void EatPlayer(string name, int mass)
    {
        var player = GetParent().GetNode<Player>(name);
        player.GrowPlayer(-mass);
    }

    public override void _Process(double delta)
    {
        starving -= delta;
        if (starving <= 0)
        {
            GrowPlayer(-PlayerSize/10);
        }
        eatenCd -= delta;
        GetNode<Label>("Label").Text = DisplayName;
        var scaled = GetNode<Node2D>("scaled");
        var collider = GetNode<CollisionShape2D>("PhysicsCollisionShape");
        scaled.Scale = scaled.Scale.Lerp(targetScale, (float) delta * 2);
        collider.Scale = scaled.Scale;
        
        var scale = Mathf.Sqrt(PlayerSize / Mathf.Pi) * 2 / 10f;
        targetScale = new Vector2(scale, scale);
        
        if (Name == Multiplayer.GetUniqueId().ToString())
        {
            DisplayServer.WindowSetTitle($"LD56 - {DisplayName} - Score: {score}" + (Multiplayer.GetUniqueId() == 1 ? " (Server)" : ""));
        }


    }

    public void _enter_tree()
    {
        if (!Name.ToString().StartsWith("AI"))
        {
            SetMultiplayerAuthority(authorityFromName());
        }
    }

    public void GrowPlayer(int mass = 200)
    {
        
        starving = 2;
        PlayerSize += mass;
        score = Mathf.Max(PlayerSize, score);
        // if (mass > 0)
        // {
            // GD.Print($"{DisplayName}({Name}) grows to {PlayerSize} (+{mass})");
        // }
        // else
        // {
            // GD.Print($"{DisplayName}({Name}) shrinks to {PlayerSize} (-{mass})");
        // }
        if (PlayerSize < 0)
        {
            PlayerDied();
        }
    }

    public void PlayerDied()
    {
        
        var world = GetParent<World>();
        world.SpawnColoredParticles(world.deathParticles, GlobalPosition, PlayerColor);
        Audio.Instance.SplatAt(GlobalPosition);    
        
        QueueFree();
        if (aiControlled)
        {
            world.aiPlayers--;
        }
        else
        {
            world.authorityPlayer = null;
            Global.Instance.SendPlayerDead();
            Global.Instance.LoadRespawnScene(score);
        }
    }
}