using Godot;
using System.Collections.Generic;

public class Level
{
    public readonly int Width;
    public readonly int Height;

    public Tile[,] Grid;

    public List<Box> Boxes = new();
    public List<Goal> Goals = new();

    public uint seed;

    public List<Tile> AllowedSpots { get; } = new();

    public Vector2I PlayerPosition;
    public Vector2I PlayerStartPosition;

    public bool Trash;

    public int SolveCounter;

    private readonly RandomNumberGenerator _rng = new();

    public Level(int width, int height, int boxCount)
    {
        Width = width;
        Height = height;

        SolveCounter = boxCount;

        Grid = new Tile[width,height];

        _rng.Randomize();


        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Grid[x,y] = new Tile(x,y);
            }
        }


        BuildAllowedSpots();
    }

    public void BuildAllowedSpots()
    {
        AllowedSpots.Clear();

        for (int x = 2; x < Width - 2; x++)
        {
            for (int y = 2; y < Height - 2; y++)
            {
                AllowedSpots.Add(Grid[x, y]);
            }
        }   
    }

    public Tile? RandomSpot()
    {
        while (AllowedSpots.Count > 0)
        {
            int index = _rng.RandiRange(0, AllowedSpots.Count - 1);

            Tile tile = AllowedSpots[index];
            AllowedSpots.RemoveAt(index);

            tile.Wall = false;

            if (!Blockaded(tile.Position))
                return tile;
        }

        return null;
    }

    public bool Blockaded(Vector2I pos)
    {
        int x = pos.X;
        int y = pos.Y;

        if (Grid[x + 1, y].ContainsBox)
        {
            if ((Grid[x + 1, y + 1].ContainsBox && Grid[x, y + 1].ContainsBox) ||
                (Grid[x + 1, y - 1].ContainsBox && Grid[x, y - 1].ContainsBox))
            {
                return true;
            }
        }

        if (Grid[x - 1, y].ContainsBox)
        {
            if ((Grid[x - 1, y - 1].ContainsBox && Grid[x, y - 1].ContainsBox) ||
                (Grid[x - 1, y + 1].ContainsBox && Grid[x, y + 1].ContainsBox))
            {
                return true;
            }
        }

        return false;
    }

    public void PlaceObjects(int boxCount)
    {
        PlaceGoals(boxCount);
        PlaceBoxes(boxCount);
        PlacePlayer();
    }

    public void Rip(int amount)
    {
        for(int i = 0; i < amount; i++)
        {
            Tile? tile = RandomSpot();

            if(tile == null)
                return;

            foreach(Vector2I dir in new[]
            {
                Vector2I.Up,
                Vector2I.Down,
                Vector2I.Left,
                Vector2I.Right
            })
            {
                Vector2I pos = tile.Position + dir;

                if(GeneratorUtils.InBounds(Grid,pos))
                    Grid[pos.X,pos.Y].Wall = false;
            }
        }
    }

    public bool CanMove(Vector2I direction)
    {

        if (IsSolved()) return false;

        Vector2I target = PlayerPosition + direction;

        if (!GeneratorUtils.InBounds(Grid, target))
            return false;

        Tile tile = Grid[target.X, target.Y];

        if (tile.Wall)
            return false;

        if (tile.ContainsBox)
            return false;

        return true;
    }

    public bool CanPush(Vector2I direction)
    {
        Vector2I boxPos = PlayerPosition + direction;

        if (!GeneratorUtils.InBounds(Grid, boxPos))
            return false;

        Tile boxTile = Grid[boxPos.X, boxPos.Y];

        if (!boxTile.ContainsBox)
            return false;

        Vector2I destination = boxPos + direction;

        if (!GeneratorUtils.InBounds(Grid, destination))
            return false;

        Tile destTile = Grid[destination.X, destination.Y];

        if (destTile.Wall)
            return false;

        if (destTile.ContainsBox)
            return false;

        return true;
    }

    public Box? GetBox(Vector2I position)
    {
        foreach(Box box in Boxes)
        {
            if(box.Position == position)
                return box;
        }

        return null;
    }

    public void MoveBox(Box box, Vector2I destination)
    {
        Tile oldTile = Grid[
            box.Position.X,
            box.Position.Y
        ];

        Tile newTile = Grid[
            destination.X,
            destination.Y
        ];


        oldTile.ContainsBox = false;
        oldTile.Occupied = false;


        box.SetPosition(destination);


        newTile.ContainsBox = true;
        newTile.Occupied = true;
    }

    public bool IsSolved()
    {
        int boxesOnGoals = 0;

        foreach(Box box in Boxes)
        {
            Tile tile = Grid[
                box.Position.X,
                box.Position.Y
            ];

            if(tile.Goal)
                boxesOnGoals++;
        }

        return boxesOnGoals == Boxes.Count;
    }

    public void Reset()
    {
        // Clear all box flags
        foreach (Box box in Boxes)
        {
            Grid[box.Position.X, box.Position.Y].ContainsBox = false;
            Grid[box.Position.X, box.Position.Y].Occupied = false;
        }

        // Move boxes back
        foreach (Box box in Boxes)
        {
            box.SetPosition(box.StartPosition);

            Grid[box.Position.X, box.Position.Y].ContainsBox = true;
            Grid[box.Position.X, box.Position.Y].Occupied = true;
        }

        PlayerPosition = PlayerStartPosition;
    }

    private void PlaceGoals(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Tile? tile = RandomSpot();

            if (tile == null)
                break;

            Goals.Add(new Goal(tile.Position));
            tile.Goal = true;
        }
    }

    private void PlaceBoxes(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Tile? tile = RandomSpot();

            if (tile == null)
                break;

            Box box = new(tile.Position, Goals[i]);

            Boxes.Add(box);

            tile.ContainsBox = true;
            tile.Occupied = true;
        }
    }

    private void PlacePlayer()
    {
        Tile? tile = RandomSpot();

        if (tile == null)
        {
            PlayerPosition = Goals[0].Position;
        }
        else
        {
            PlayerPosition = tile.Position;
        }

        PlayerStartPosition = PlayerPosition;
    }
}