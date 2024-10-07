using Godot;
using LD56;

public partial class Respawn : CanvasLayer
{
	public override void _Process(double delta)
	{
		if (Input.GetActionStrength("respawn") > 0.5)
		{
			Global.Instance.LoadCountdownSceneInWorld();
		}
	}
}
