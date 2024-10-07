using Godot;
using System.Collections.Generic;
using System.Linq;
using LD56;

public partial class MassIndicator : Node2D
{
	private Player player;
	private Node2D scaled;
	private IList<Sprite2D> indicators;
	
	public override void _Ready()
	{
		player = GetParent<Player>();
		if (Multiplayer.GetUniqueId().ToString() != player.Name)
		{
			QueueFree();
			return;
		}

		Visible = true;
		scaled = player.GetNode<Node2D>("scaled");
		indicators = GetChildren().Select(n => (Sprite2D)n).ToList();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!Visible)
		{
			return;
		}
		Scale = scaled.Scale;
		
		var massContributors = GetTree().GetNodesInGroup("MassContributor");

		var playerPos = player.GlobalPosition;
		var indicatorsCount = indicators.Count;
		var angleBuckets = new double[indicatorsCount];
		var bucketSize = 360d / indicatorsCount;
		foreach (var node in massContributors)
		{
			if (node != player && node is Node2D node2d && node is MassContributor massContributor)
			{
				var contributorDir = node2d.GlobalPosition - playerPos;
				var contributorAngle = Mathf.RadToDeg(Mathf.Atan2(contributorDir.Y, contributorDir.X)) + 180;
				var angleBucket = (int)(contributorAngle / bucketSize);
				angleBuckets[angleBucket] += massContributor.ContributedMass;
			}
		}
		
		var minMass = double.MaxValue;
		var maxMass = double.MinValue;
		foreach (var mass in angleBuckets)
		{
			if (mass < minMass)
			{
				minMass = mass;
			}

			if (mass > maxMass)
			{
				maxMass = mass;
			}
		}

		var massRange = maxMass - minMass;
		
		for (var i = 0; i < angleBuckets.Length; i++)
		{
			var massFraction = (angleBuckets[i] - minMass) / massRange;
			var indicator = indicators[i];
			(indicator.Material as ShaderMaterial)?.SetShaderParameter("arc_color", Color.FromHsv(0, (float)massFraction, 1, 0.5f + ((float)massFraction / 2f)));
		}
	}
}
