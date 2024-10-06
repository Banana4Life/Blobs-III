using Godot;

public partial class Player : CharacterBody2D
{
    public const float SPEED = 300.0f;

    [Export] public string DisplayName;
    [Export] public int PlayerSize;
    [Export] public Vector2 targetScale;


    public override void _Ready()
    {
        // var syncher = GetNode<MultiplayerSynchronizer>("PlayerSync");
        // syncher.SetVisibilityFor(0, false);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsMultiplayerAuthority())
        {
            Velocity = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down") * SPEED;
            MoveAndSlide();
        }

        GetNode<Label>("Label").Text = DisplayName;
    }

    public override void _Process(double delta)
    {
        var scaled = GetNode<Node2D>("scaled");
        scaled.Scale = scaled.Scale.Lerp(targetScale, (float) delta);
    }

    public void _enter_tree()
    {
        SetMultiplayerAuthority(int.Parse(Name));
    }

    public void GrowPlayer(int size = 200)
    {
        PlayerSize += size;
        var scale = Mathf.Sqrt(PlayerSize / Mathf.Pi) * 2 / 10f;
        targetScale = new Vector2(scale, scale);
        GD.Print($"{DisplayName} grows to {PlayerSize} {scale}");
    }
}