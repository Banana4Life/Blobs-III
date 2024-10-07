using Godot;
using LD56;

public partial class Toast : CanvasLayer
{
	private SimpleTimer timer = new(5, false);
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Hide();
		GetNode<Button>("col/row/container/player/btn").Hide();
		Present(UnlockableColors.PickRandomColorName());
	}

	public void Present(string name)
	{
		GetNode<Button>("col/row/container/player/btn").Hide();
		GetNode<PlayerSelector>("col/row/container/player").SetColor(name);
		timer.Reset();
		Show();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Visible && timer.Update(delta))
		{
			Hide();
		}
	}
}
