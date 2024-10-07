using Godot;
using System;
using System.Linq;
using LD56;

public partial class PlayerSelector : Control
{
	public override void _Ready()
	{
		updateColor();
	}

	public void SetColor(string name)
	{
		GetNode<Label>("Control/Label").Text = name;
		var sprite2D = GetNode<Sprite2D>("Control/Sprite2D");
		var unlockableColor = UnlockableColors.Colors[name];
		if (unlockableColor.Material != null)
		{
			sprite2D.Material = unlockableColor.Material;
		}
		
		var material = sprite2D.Material as ShaderMaterial;
		material.SetShaderParameter("bodyColor", unlockableColor.Color);
	}

	public void updateColor()
	{
		SetColor(Global.Instance.selectedColor);
	}

	public void _on_btn_pressed()
	{
		var indexOf = Global.Instance.unlockedColors.IndexOf(Global.Instance.selectedColor);
		indexOf++;
		if (indexOf >= Global.Instance.unlockedColors.Count())
		{
			indexOf = 0;
		}

		Global.Instance.selectedColor = Global.Instance.unlockedColors[indexOf];
		
		updateColor();
	}
}
