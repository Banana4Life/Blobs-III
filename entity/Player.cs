using System;
using System.Linq;
using Godot;
using LD56;

public partial class Player : CharacterBody2D, MassContributor
{
    private PackedScene deathParticles = GD.Load<PackedScene>("res://particles/death_particles.tscn");
    
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

    private void detectCollision(double delta)
    {
        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            var collision = GetSlideCollision(i);
            // GD.Print($"{DisplayName} collided {collision.GetCollider()}");

            if (collision.GetCollider() is RigidBody2D rb)
            {
                rb.ApplyCentralImpulse(-collision.GetNormal() * 5);
            }
            if (collision.GetCollider() is Particle pa)
            {
                if (pa.size < PlayerSize)
                {
                    if (pa.eatenCd < 0)
                    {
                        pa.eatenCd = 0.1;
                        var massEaten = (int) (Mathf.Max(5, pa.size * delta * 25));
                        GrowPlayer(Mathf.Max(1, massEaten / 2));
                        RpcId(1, MethodName.EatParticle, pa.Name, massEaten);    
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
        particle.size -= mass;
        
        var world = GetParent<World>();
        if (particle.tiny)
        {
            world.totalTinyMass -= mass;
        }
        else
        {
            world.totalMass -= mass;
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
        scaled.Scale = scaled.Scale.Lerp(targetScale, (float) delta);
        collider.Scale = scaled.Scale;
        
        var scale = Mathf.Sqrt(PlayerSize / Mathf.Pi) * 2 / 10f;
        targetScale = new Vector2(scale, scale);
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
        var particle = deathParticles.Instantiate<GpuParticles2D>();
        if (particle.ProcessMaterial is ParticleProcessMaterial particleProcessMaterial)
        {
            var gradient = new Gradient();
            gradient.AddPoint(0.0f, Colors.Pink);
            gradient.AddPoint(1.0f, Colors.Black);

            var gradientTexture = new GradientTexture2D();
            gradientTexture.Gradient = gradient;
            particleProcessMaterial.SetColorRamp(gradientTexture);

            particle.GlobalPosition = GlobalPosition;
            particle.Emitting = true;
            
            GetParent<World>().AddChild(particle);
        }
        
        QueueFree();
        if (aiControlled)
        {
            GetParent<World>().aiPlayers--;
        }
        else
        {
            GetParent<World>().authorityPlayer = null;
            Global.Instance.SendPlayerDead();
            Global.Instance.LoadRespawnScene(score);
        }
    }
}