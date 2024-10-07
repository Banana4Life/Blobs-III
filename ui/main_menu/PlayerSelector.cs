using Godot;
using System;
using LD56;

public partial class PlayerSelector : Control
{


	public override void _Ready()
	{
		updateColor();
	}

	public void updateColor()
	{
		GetNode<Label>("Control/Label").Text = Global.Instance.selectedColor;
		var material = GetNode<Sprite2D>("Control/Sprite2D").Material as ShaderMaterial;
		material.SetShaderParameter("bodyColor", UnlockableColors.Colors[Global.Instance.selectedColor].Color);
	}

	public void _on_btn_pressed()
	{
		var indexOf = Array.IndexOf(Global.Instance.unlockedColors, Global.Instance.selectedColor);
		indexOf++;
		if (indexOf >= Global.Instance.unlockedColors.Length)
		{
			indexOf = 0;
		}

		Global.Instance.selectedColor = Global.Instance.unlockedColors[indexOf];
		
		updateColor();
	}
}
