using Godot;

public class Box
{
    public Vector2I Position;
    public Vector2I StartPosition;
    public Goal Goal;
    public BoxController Controller;

    public bool Placed;

    public Box(Vector2I position, Goal goal)
    {
        Position = position;
        StartPosition = position;
        Goal = goal;
    }

    public void SetPosition(Vector2I position)
    {
        Position = position;
    }
}