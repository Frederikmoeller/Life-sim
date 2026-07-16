using Godot;
using System.Collections.Generic;

public partial class SokobanGenerator : Node2D
{
    [Export] public PackedScene WallScene;
    [Export] public PackedScene FloorScene;
    [Export] public PackedScene BoxScene;
    [Export] public PackedScene GoalScene;
    [Export] public PackedScene PlayerScene;

    public Dictionary<Box, BoxController> BoxControllers = new();

    private int TileSize = 32;
    private readonly RandomNumberGenerator _rng = new();

    public Level GenerateLevel(int size, int boxes)
    {
        uint seed = GD.Randi();
        _rng.Seed = seed;

        while(true)
        {
            Level level = new(size, size, boxes);

            level.PlaceObjects(boxes);

            Generator.GeneratePaths(level);

            if(level.Trash)
                continue;

            level.Rip(_rng.RandiRange(2,8));

            level.seed = seed;

            Render(level);

            return level;
        }
    }

    public void ResetLevel(Level level)
    {
        Render(level);
    }

    private void Render(Level level)
    {
        foreach (Node child in GetChildren())
        {
            child.QueueFree();
        }

        BoxControllers.Clear();

        for (int x = 0; x < level.Width; x++)
        {
            for (int y = 0; y < level.Height; y++)
            {
                Tile tile = level.Grid[x, y];

                Vector2 pos = new Vector2(x * TileSize, y * TileSize);

                if (tile.Wall)
                {
                    Node2D wall = WallScene.Instantiate<Node2D>();

                    AddChild(wall);
                    wall.Position = pos;
                }
                else
                {
                    Node2D floor = FloorScene.Instantiate<Node2D>();

                    AddChild(floor);
                    floor.Position = pos;
                }
            }
        }

        foreach(Box box in level.Boxes)
        {
            BoxController node =
                BoxScene.Instantiate<BoxController>();

            AddChild(node);

            node.Initialize(box);
            BoxControllers.Add(box,node);
        }

        foreach (Goal goal in level.Goals)
        {
            Node2D node = GoalScene.Instantiate<Node2D>();

            AddChild(node);

            node.Position = new Vector2(goal.Position.X * TileSize, goal.Position.Y * TileSize);
        }

        PlayerController player = PlayerScene.Instantiate<PlayerController>();

        AddChild(player);

        player.Initialize(level, BoxControllers);

        player.Position = new Vector2(level.PlayerPosition.X * TileSize, level.PlayerPosition.Y * TileSize);
    }
}