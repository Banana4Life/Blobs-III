using Godot;
using System;
using LD56;

public partial class Respawn : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.GetActionStrength("respawn") > 0.5)
		{
			Global.Instance.LoadCountdownSceneInWorld();
		}
	}
}
