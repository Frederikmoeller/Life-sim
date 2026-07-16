using Godot;
using System;

public static class GridUtils
{
    public const int TileSize = 32;

    public static Vector2 GridToWorld(Vector2I cell)
    {
        return cell * TileSize;
    }
}