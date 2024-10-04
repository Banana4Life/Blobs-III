using Godot;
using System;

public partial class Player : CharacterBody2D
{
	public const float SPEED = 300.0f;

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
		SetMultiplayerAuthority(int.Parse(Name));
	}
}
