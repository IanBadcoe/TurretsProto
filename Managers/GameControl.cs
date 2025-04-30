using Godot;

public class GameControl
{
    public bool Start { get; private set; } = false;

    public void Process()
    {
        if (Input.IsActionJustPressed("Start"))
        {
            Start = true;
        }
    }
}