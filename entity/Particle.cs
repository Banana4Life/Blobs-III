using Godot;
using LD56;

public partial class Particle : RigidBody2D, MassContributor
{
    private bool validSpawn;
    private double aliveTime;
    [Export] public int size;
    private Vector2 targetScale;
    [Export] public Color Color;
    [Export] public float seed;
    [Export] public float mag;
    [Export] public float freq;

    public int ContributedMass => size;
    public double eatenCd;
    private RandomNumberGenerator random =  Global.Instance.Random;
    public bool tiny;
    
    public override void _Ready()
    {
        var syncher = GetNode<MultiplayerSynchronizer>("ParticleSync");
        syncher.SetVisibilityFor(0, false);
        
        var sprite = GetNode<Sprite2D>("scaled/Sprite2D");
        sprite.Visible = false;
        if (sprite.Material is ShaderMaterial shaderMat)
        {
            shaderMat.SetShaderParameter("bodyColor", Color);
            shaderMat.SetShaderParameter("cellColor", Colors.Black);
            shaderMat.SetShaderParameter("uSeed", seed);
            shaderMat.SetShaderParameter("uMagnitude", mag);
            shaderMat.SetShaderParameter("uFrequency", freq);
        }
    }

    public void RandomInit(int size)
    {
        this.size = size;
        Shrink(0); // update targetscale
        var color = Color.FromHsv(random.RandfRange(0, 1f), 1f, 1f, random.RandfRange(0.2f, 0.4f));
        Color = color;
        seed = random.RandfRange(0f, 10000f);
        mag = random.RandfRange(0.01f, 0.19f);
        freq = random.RandfRange(0.5f, 5.5f);
    }

    public void Despawn()
    {
        var world = GetParent<World>();
        if (tiny)
        {
            world.totalTinyMass -= size;
        }
        else
        {
            world.totalMass -= size;
        }

        QueueFree();
    }

    public void Die()
    {
        QueueFree();
        var world = GetParent<World>();
        world.SpawnColoredParticles(world.deathParticles, GlobalPosition, Color);
        if (tiny)
        {
            Audio.Instance.SplatAt(GlobalPosition, volumeLinear:0.03f);
        }
        else
        {
            Audio.Instance.SplatAt(GlobalPosition, volumeLinear:0.08f);
        }
    }

    public void _on_area_2d_area_entered(Area2D area)
    {
        var otherParent = area.GetParent().GetParent(); 
        if (otherParent is Particle otherParticle) // Spawning particle
        {
            if (validSpawn)
            {
                if (!otherParticle.validSpawn)
                {
                    otherParticle.Despawn();
                }
                else
                {
                    // GD.Print("Both are valid?");
                }
            }
            else if (otherParticle.validSpawn)
            {
                Despawn();
            }
            else
            {
                validSpawn = true;
                otherParticle.Despawn();
            }
        }
        else if (otherParent is Player player)
        {
            if (!validSpawn)
            {
                var sprite = GetNode<Sprite2D>("scaled/Sprite2D");
                // sprite.Visible = false;
                if (sprite.Material is ShaderMaterial shaderMat)
                {
                    shaderMat.SetShaderParameter("bodyColor", Godot.Colors.Red);
                }
                // GD.Print("Despawn in player..");
                Despawn();
            }
        }
        else
        {
            GD.PrintErr($"unhandled area entered {area} {this}", otherParent.Name);
        }
    }

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        state.LinearVelocity = state.LinearVelocity.Lerp(new Vector2(random.Randf() - 0.5f, random.Randf() - 0.5f) * 50f, 0.1f);
    }

    public override void _Process(double delta)
    {
        if (size <= 0)
        {
            Die();
        }
        
        eatenCd -= delta;
        aliveTime += delta;
        if (aliveTime >= 0.5d)
        {
            if (!validSpawn)
            {
                GetNode<Node2D>("scaled").Scale = new Vector2(0,0); // Reset size for spawning
            }
            validSpawn = true;
            // GD.Print("Spawned particle " + size);
            GetNode<CollisionShape2D>("PhysicsCollisionShape").Disabled = false;
            var syncher = GetNode<MultiplayerSynchronizer>("ParticleSync");
            syncher.SetVisibilityFor(0, true);

        }
        
        GetNode<Sprite2D>("scaled/Sprite2D").Visible = validSpawn;
        if (validSpawn && Multiplayer.IsServer())
        {
            var newScale = GetNode<Node2D>("scaled").Scale.Lerp(targetScale, 0.1f);
            GetNode<Node2D>("scaled").Scale = newScale;
            GetNode<CollisionShape2D>("PhysicsCollisionShape").Scale = newScale;
        }

        
    }

    public void Shrink(int mass)
    {
        size -= mass;
        var scale = Mathf.Sqrt(size / Mathf.Pi) * 2 / 10f;
        targetScale = new Vector2(scale, scale);
        if (tiny)
        {
            var world = GetParent<World>();
            if (tiny)
            {
                world.totalTinyMass -= mass;
            }
            else
            {
                world.totalMass -= mass;
            }
        }
    }
}