using Godot;
using LD56;

public partial class Countdown : CanvasLayer
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
		if (timeLeft > 0)
		{
			label.Text = timeLeft.ToString("0");
			return;
		}

		GD.Print("Countdown finished");
		Audio.Instance.BackgroundVolumeLinear = 0.05f;
		Global.Instance.LoadWorldScene(true);
		if (Global.Instance.State is ClientState)
		{
			Global.Instance.SendPlayerReady();
		}
	}
}
