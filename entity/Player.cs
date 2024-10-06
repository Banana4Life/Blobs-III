using Godot;

public partial class Player : CharacterBody2D
{
    public const float SPEED = 300.0f;

    [Export] public string DisplayName;
    [Export] public int PlayerSize;


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
        Scale = new Vector2(PlayerSize / 100f, PlayerSize / 100f);
    }

    public void _enter_tree()
    {
        SetMultiplayerAuthority(int.Parse(Name));
    }

    public void Grow(int size)
    {
        PlayerSize += (int)Mathf.Sqrt(size);
    }
}