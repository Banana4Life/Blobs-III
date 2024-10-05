using Godot;
using System;
using Environment = Godot.Environment;

public partial class Player : CharacterBody2D
{
	public const float SPEED = 300.0f;

	[Export] public int peerId;
	
	public override void _PhysicsProcess(double delta)
	{
		if (IsMultiplayerAuthority())
		{
			Velocity = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down") * SPEED;
			MoveAndSlide();
		}
		
	}

	public void _enter_tree()
	{
		if (peerId != 0)
		{
			GD.Print($"{Name} Entered tree");
			SetMultiplayerAuthority(peerId);	
		}
		else
		{
			GD.Print($"{Name} Entered tree with auth");
		}
	}
}
