using Godot;

namespace LD56;

public partial class Global : Node
{
    public static Global Instance { get; private set; }

    public State State;

    public override void _Ready()
    {
        Instance = this;
    }
    
    public override void _Process(double delta)
    {
        if (State != null)
        {
            State.Update(delta);
        }
    }

}