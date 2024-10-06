using Godot;
using System;
using Godot.Collections;

public partial class Particle : Node2D
{
    private bool validSpawn = false;
    private double aliveTime = 0;
    public int size;


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
        }

        GetNode<Sprite2D>("Sprite2D").Visible = validSpawn;
    }
}