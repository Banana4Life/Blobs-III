using Godot;
using System;

public partial class World : Node2D
{
    [Export] public PackedScene particlePrefab;

    private int players = 1;
    public int totalMass = 0;
    public int maxMass = 1000000;
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
        var randScale = random.RandfRange(30f, 150f);
        // var randScale = random.RandfRange(150f, 150f);
        sprite.Scale = new Vector2(randScale, randScale);

        var color = Color.FromHsv(random.RandfRange(0, 1f), 1f, 1f, 1f);
        (sprite.Material as ShaderMaterial).SetShaderParameter("bodyColor", color);
        (sprite.Material as ShaderMaterial).SetShaderParameter("cellColor", color);
        // TODO when bigger reduct magnitude
        (sprite.Material as ShaderMaterial).SetShaderParameter("uSeed", random.RandfRange(0f, 10000f));
        (sprite.Material as ShaderMaterial).SetShaderParameter("uMagnitude", random.RandfRange(0.01f, 0.19f));
        (sprite.Material as ShaderMaterial).SetShaderParameter("uFrequency", random.RandfRange(0.5f, 5.5f));
        // GD.Print($"spawn {i++} at {spawnPos} {color.H}");


        AddChild(particle);
        return (int)(3 * (randScale * randScale) / 4);
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
        spawnToMaxMass();
    }
}