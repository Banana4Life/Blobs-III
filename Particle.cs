using Godot;
using System;
using Godot.Collections;

public partial class Particle : Node2D
{
    private bool validSpawn = false;
    private double aliveTime = 0;
    [Export] public int size;
    [Export] public Color Color;
    [Export] public float seed;
    [Export] public float mag;
    [Export] public float freq;
    

    public override void _Ready()
    {
        var syncher = GetNode<MultiplayerSynchronizer>("Syncher");
        syncher.SetVisibilityFor(0, false);
        
        var sprite = GetNode<Sprite2D>("Sprite2D");
        var shaderMat = (sprite.Material as ShaderMaterial);
        shaderMat.SetShaderParameter("bodyColor", Color);
        shaderMat.SetShaderParameter("cellColor", Colors.Black);
        shaderMat.SetShaderParameter("uSeed", seed);
        shaderMat.SetShaderParameter("uMagnitude", mag);
        shaderMat.SetShaderParameter("uFrequency", freq);
    }


    public void RemoveFromGame()
    {
        if (!IsQueuedForDeletion())
        {
            var world = (World)GetParent();
            world.totalMass -= size;
        }

        QueueFree();
    }

    public void _on_area_2d_area_entered(Area2D area)
    {
        var otherParticle = area.GetParent<Particle>(); // TODO other things that collide?
        if (validSpawn)
        {
            if (!otherParticle.validSpawn)
            {
                otherParticle.RemoveFromGame();
            }
            else
            {
                // TODO how does this happen?
                // GD.Print("Both are valid?");
            }
        }
        else if (otherParticle.validSpawn)
        {
            RemoveFromGame();
        }
        else
        {
            validSpawn = true;
            otherParticle.RemoveFromGame();
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        aliveTime += delta;
        if (aliveTime >= 0.5d)
        {
            validSpawn = true;
            
            var syncher = GetNode<MultiplayerSynchronizer>("Syncher");
            syncher.SetVisibilityFor(0, true);
        }

        GetNode<Sprite2D>("Sprite2D").Visible = validSpawn;
    }
}