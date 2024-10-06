using Godot;
using System.Globalization;

public partial class Countdown : Control
{
	private double timer;
	[Export] private Label label;
	[Export] private PackedScene worldScene;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		timer = 0;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		timer += delta;
		var timeLeft = 3 - timer;
		if (timeLeft < 0)
		{
			GetTree().Root.AddChild(worldScene.Instantiate());
			Visible = false;
			// TODO: load world
			return;
		}

		label.Text = ((int)timeLeft).ToString(CultureInfo.InvariantCulture);
	}
}
