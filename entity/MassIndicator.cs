using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MassIndicator : Node2D
{
	private Player player;
	private Node2D scaled;
	private IList<Sprite2D> indicators;
	
	public override void _Ready()
	{
		player = GetParent<Player>();
		scaled = player.GetNode<Node2D>("scaled");
		indicators = GetChildren().Select(n => (Sprite2D)n).ToList();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Scale = scaled.Scale;
		var massContributors = GetTree().GetNodesInGroup("MassContributors");
	}
}
