using Godot;

public static class GeneratorUtils
{
    public static bool InBounds(Tile[,] grid, Vector2I pos)
    {
        return pos.X >= 0 &&
               pos.Y >= 0 &&
               pos.X < grid.GetLength(0) &&
               pos.Y < grid.GetLength(1);
    }
}