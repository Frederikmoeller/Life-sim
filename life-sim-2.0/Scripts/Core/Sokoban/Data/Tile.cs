using Godot;
using System;


public class Tile
{
    public Vector2I Position { get; }

    // Tile information
    public bool Wall = true;
    public bool Occupied;
    public bool ContainsBox;
    public bool Goal;
    public bool Used;

    // Pathfinding
    public float F;
    public float Cost;
    public bool Closed;
    public bool Checked;
    public Tile? Parent;

    public Tile(int x, int y)
    {
        Position = new Vector2I(x, y);
    }

    public void ResetPathfinding()
    {
        F = 0;
        Cost = 0;
        Closed = false;
        Checked = false;
        Parent = null;
    }
}
