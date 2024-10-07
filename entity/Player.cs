using System;
using System.Linq;
using Godot;
using LD56;

public partial class Player : CharacterBody2D, MassContributor
{
    public static readonly PackedScene eatParticles = GD.Load<PackedScene>("res://particles/eat_particles.tscn");

    
    public const float SPEED = 300.0f;
    public float currentSpeed = SPEED;
    public bool dashing;

    public bool aiControlled;
    
    [Export] public string UnlockableColorName;
    [Export] public string DisplayName;
    [Export] public int PlayerSize;
    [Export] public int score;
    public Vector2 targetScale;

    public int ContributedMass => PlayerSize;
    public double eatenCd;
    private double starving = 5;
    public UnlockableColor Color;
    
    public override void _Ready()
    {
        Color = UnlockableColors.Colors[UnlockableColorName];
        GetNode<Node2D>("scaled").Scale = Vector2.Zero;
        // var syncher = GetNode<MultiplayerSynchronizer>("PlayerSync");
        // syncher.SetVisibilityFor(0, false);
        var sprite2D = GetNode<Sprite2D>("scaled/Sprite2D");
        if (Color.Material != null)
        {
            sprite2D.Material = Color.Material;
        }

        (sprite2D.Material as ShaderMaterial)
            ?.SetShaderParameter("bodyColor", Color.Color);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void Bump(string name, Vector2 direction)
    {
        if (GetParent().HasNode(name))
        {
            var node = GetParent().GetNode(name);
            if (node is Player pl)
            {
                pl.Velocity = direction * SPEED * 3;    
            }

            if (node is Particle particle)
            {
                particle.ApplyCentralImpulse(direction);
            }
        }
        else
        {
            // TODO missing node?
        }
        
        
    }
    
    public override void _PhysicsProcess(double delta)
    {
        if (Multiplayer.IsServer() && aiControlled)
        {
            var massContributors = GetTree().GetNodesInGroup("MassContributor");
            var players = massContributors.Where(mc => mc is Player { aiControlled: false }).ToArray();
            var max = players.Length == 0 ? 0 : players.Max(mc => ((Player)mc).PlayerSize);
            max = Math.Max(max, 450);
            if (PlayerSize <= max)
            {
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
                    }).OrderBy(mc => (((Node2D)mc).GlobalPosition - GlobalPosition).LengthSquared())
                    .ToArray();
                if (sorted.Length == 0)
                {
                    if (starving < 0)
                    {
                        GD.Print($"{Name} cannot find any prey");
                        PlayerDied();
                    }

                    return;
                }

                var newVelocity = (((Node2D)sorted.First()).GlobalPosition - GlobalPosition).Normalized() * Speed();
                Velocity = Velocity.Lerp(newVelocity, 0.1f);
            }
            else
            {
                // TODO do stuff when player size was reached
            }
        }
        else
        {
            if (IsMultiplayerAuthority())
            {
                if (dashing)
                {
                    currentSpeed -= (float) (delta * SPEED * 1.5);
                    if (currentSpeed < SPEED)
                    {
                        currentSpeed = SPEED;
                        dashing = false;
                    }
                }

                if (!dashing && Input.GetActionStrength("dash") > 0.5)
                {
                    dashing = true;
                    currentSpeed = SPEED * 3;
                    GrowPlayer(-Math.Max(10, PlayerSize / 10));
                }

                var newVelocity = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down") * Speed();
                Velocity = Velocity.Lerp(newVelocity, 0.1f);
            }
        }
        MoveAndSlide();
        DetectCollision(delta);
    }

    private float Speed()
    {
        return Math.Max(150, currentSpeed - PlayerSize / 100f);
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
    
    
    
    private void DetectCollision(double delta)
    {
        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            var collision = GetSlideCollision(i);
            // GD.Print($"{DisplayName} collided {collision.GetCollider()}");

            if (collision.GetCollider() is Particle particle)
            {
                Vector2 impulse = Vector2.Zero;
                if (particle.size < PlayerSize)
                {
                    float dashingMulti = dashing ? 10 : 1;
                    impulse = -collision.GetNormal() * 10 * dashingMulti;
                }
                else
                {
                    float dashingMulti = dashing ? 50 : 1;
                    // var ratio = Mathf.Max(1, PlayerSize / particle.size);
                    impulse = -collision.GetNormal() * 5 * dashingMulti;
                }
                RpcId(1, MethodName.Bump, particle.Name, impulse);  // Notify server we bumped
                particle.ApplyCentralImpulse(impulse); // also apply locally (for prediction)

                if (!Multiplayer.IsServer())
                {
                }
            }
      
            if (collision.GetCollider() is Player player && dashing)
            {
                // GD.Print(DisplayName, "dash into ", player.DisplayName, player.AuthorityFromName(), collision.GetNormal());
                RpcId(player.AuthorityFromName(), MethodName.Bump, player.Name, -collision.GetNormal().Normalized());
            }
            
            
            if (dashing) // Cannot eat while dashing
            {
                return;
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
                        GrowPlayer(Mathf.Max(1, massEaten / (pa.tiny ? 1 : 2)));
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
                        if (!aiControlled && pl.PlayerSize <= massEaten)
                        {
                            Global.Instance.AwardUnlockedColor(pl.UnlockableColorName);
                        }
                        GrowPlayer(Mathf.Max(1, massEaten / 4));
                        // GD.Print($"{Multiplayer.GetUniqueId()} : {DisplayName} eats {massEaten} of {pl.DisplayName}");
                            
                        RpcId(pl.AuthorityFromName(), MethodName.EatPlayer, pl.Name, massEaten);
                        pl.eatenCd = 0.1;
                        
                        SpawnColoredParticlesOnScaled(pl, eatParticles, collision.GetPosition(), pl.Color.Color);
                    }
                }
            }
        }
    }

    public int AuthorityFromName()
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
        if (GetParent().HasNode(name))
        {
            var particle = GetParent().GetNode<Particle>(name);
            if (particle != null && particle.validSpawn)
            {
                particle.Shrink(mass);
            }    
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
            GrowPlayer(Math.Min(-PlayerSize/10, -1));
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
            SetMultiplayerAuthority(AuthorityFromName());
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
        world.SpawnColoredParticles(world.deathParticles, GlobalPosition, Color.Color);
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