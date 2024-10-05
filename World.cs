using Godot;
using System;

public partial class World : Node2D
{
    [Export] public PackedScene particlePrefab;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var spawnRange = GetViewport().GetVisibleRect().Size;
        var random = new RandomNumberGenerator();
        // TODO only on server
        
        for (int i = 0; i < 1000; i++)
        {
            var particle = particlePrefab.Instantiate();
            var spawnPos = new Vector2(
                random.RandfRange(0f, spawnRange.X),
                random.RandfRange(0f, spawnRange.Y));
            
      

            ((Node2D)particle).GlobalPosition = spawnPos;
            var sprite = particle.GetNode<Sprite2D>("Sprite2D");
            var randScale = random.RandfRange(10f, 30f);
            sprite.Scale = new Vector2(randScale, randScale);

            var color = Color.FromHsv(random.RandfRange(0, 1f), 1f, 1f, 1f);
            (sprite.Material as ShaderMaterial).SetShaderParameter("color", color);
            // GD.Print($"spawn at {spawnPos} {color.H}");


            AddChild(particle);
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}