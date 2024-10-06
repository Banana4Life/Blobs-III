using Godot;

public partial class Player : CharacterBody2D
{
    public const float SPEED = 300.0f;

    [Export] public string DisplayName;
    [Export] public int PlayerSize;
    public Vector2 targetScale;


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
            for (int i = 0; i < GetSlideCollisionCount(); i++)
            {
                var collision = GetSlideCollision(i);
                // GD.Print($"{DisplayName} collided {collision.GetCollider()}");

                if (collision.GetCollider() is RigidBody2D rb)
                {
                    rb.ApplyCentralImpulse(-collision.GetNormal() * 5);
                }
                if (collision.GetCollider() is Particle p)
                {
                    if (p.size < PlayerSize)
                    {
                        GrowPlayer(p.size);
                        p.RemoveFromGame();
                    }
                }
            }
        }
    }


    public override void _Process(double delta)
    {
        GetNode<Label>("Label").Text = DisplayName;
        var scaled = GetNode<Node2D>("scaled");
        var collider = GetNode<CollisionShape2D>("PhysicsCollisionShape");
        scaled.Scale = scaled.Scale.Lerp(targetScale, (float) delta);
        collider.Scale = scaled.Scale;
        
        
        var scale = Mathf.Sqrt(PlayerSize / Mathf.Pi) * 2 / 10f;
        targetScale = new Vector2(scale, scale);


    }

    public void _enter_tree()
    {
        SetMultiplayerAuthority(int.Parse(Name));
    }

    public void GrowPlayer(int size = 200)
    {
        PlayerSize += size;
        GD.Print($"{DisplayName} grows to {PlayerSize}");
    }
}