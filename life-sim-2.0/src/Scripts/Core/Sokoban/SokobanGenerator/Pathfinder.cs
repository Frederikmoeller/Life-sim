using Godot;
using System;
using System.Collections.Generic;

public class Pathfinder
{
    private readonly Level _level;
    private readonly Tile[,] _grid;

    List<Tile> touched = new();

    private readonly Vector2I _start;
    private readonly Vector2I _goal;

    private const int WallCost = 100;
    private const int FloorCost = 1;
    private const int PlayerFloorCost = -1;
    private const int BoxCost = 10000;

    public Pathfinder(Level level, Vector2I start, Vector2I goal)
    {
        _level = level;
        _grid = level.Grid;

        _start = start;
        _goal = goal;
    }

    public PathResult FindPath(bool isBox)
    {
        PriorityQueue<Tile, float> open = new();

        List<Tile> visited = new();

        Tile startTile = _grid[_start.X, _start.Y];

        open.Enqueue(startTile, 0);
        touched.Add(startTile);

        while (open.Count > 0)
        {
            Tile current = open.Dequeue();

            if (current.Closed)
                continue;

            current.Closed = true;

            visited.Add(current);

            if (current.Position == _goal)
            {
                return BuildPath(current, visited);
            }

            CheckNeighbour(current, Vector2I.Right, open, visited, isBox);
            CheckNeighbour(current, Vector2I.Left, open, visited, isBox);
            CheckNeighbour(current, Vector2I.Up, open, visited, isBox);
            CheckNeighbour(current, Vector2I.Down, open, visited, isBox);
        }

        return new PathResult();
    }

    private void CheckNeighbour(
        Tile parent,
        Vector2I direction,
        PriorityQueue<Tile, float> open,
        List<Tile> visited,
        bool isBox)
    {
        Vector2I pos = parent.Position + direction;

        if (!GeneratorUtils.InBounds(_grid, pos))
            return;


        Tile neighbour = _grid[pos.X, pos.Y];


        if (neighbour.Closed)
            return;


        float newCost = CalculateCost(neighbour, parent, isBox);


        if (!neighbour.Checked)
        {
            touched.Add(neighbour);
            neighbour.Checked = true;
            neighbour.Parent = parent;
            neighbour.Cost = newCost;

            neighbour.F =
                neighbour.Cost +
                Mathf.Abs(pos.X - _goal.X) +
                Mathf.Abs(pos.Y - _goal.Y);

            open.Enqueue(neighbour, neighbour.F);
        }
        else if(!neighbour.Closed &&
                newCost < neighbour.Cost &&
                neighbour.Parent?.Parent != null)
        {
            neighbour.Parent = parent;
            neighbour.Cost = newCost;

            neighbour.F =
                neighbour.Cost +
                Mathf.Abs(pos.X - _goal.X) +
                Mathf.Abs(pos.Y - _goal.Y);

            open.Enqueue(neighbour, neighbour.F);
        }
    }

    private PathResult BuildPath(Tile endTile, List<Tile> visited)
    {
        PathResult result = new();

        result.Found = true;
        result.Cost = endTile.Cost;

        Tile? current = endTile;

        while (current?.Parent != null)
        {
            result.Path.Insert(0, current);
            current = current.Parent;
        }

        foreach (Tile tile in touched)
            tile.ResetPathfinding();

        return result;
    }

    private float CalculateCost(Tile node, Tile parent, bool isBox)
    {
        float cost;

        if (node.Occupied)
        {
            cost = parent.Cost + BoxCost;
        }
        else
        {
            if (isBox)
                cost = node.Wall ? parent.Cost + WallCost
                                : parent.Cost + FloorCost;
            else
                cost = node.Wall ? parent.Cost + WallCost
                                : parent.Cost + PlayerFloorCost;
        }

        if (isBox)
        {
            if (parent.Parent != null)
                cost += CalculateTurningCost(node, parent);
            else
                cost += CalculateInitialPushCost(node, parent);
        }

        if (node.Used)
            cost -= 5;

        return cost;
    }

    private int NodeCost(Vector2I position)
    {
        if (!GeneratorUtils.InBounds(_grid, position))
            return BoxCost;

        Tile tile = _grid[position.X, position.Y];

        if (tile.Occupied)
            return BoxCost;

        return tile.Wall
            ? WallCost
            : PlayerFloorCost;
    }

    private int CalculateInitialPushCost(Tile node, Tile parent)
    {
        Vector2I direction = node.Position - parent.Position;

        return NodeCost(node.Position + direction);
    }

    private int CalculateTurningCost(Tile node, Tile parent)
    {
        int cost1 = 0;
        int cost2 = 0;


        // Case 1:
        // node is right of parent
        if (node.Position.X - 1 == parent.Position.X &&
            node.Position.X - 2 != parent.Parent.Position.X)
        {
            if (node.Position.Y - 1 == parent.Parent.Position.Y)
            {
                // node is up-right of parent.parent

                cost1 =
                    NodeCost(node.Position + new Vector2I(-2, 0)) +
                    NodeCost(node.Position + new Vector2I(-2, -1));


                cost2 =
                    NodeCost(node.Position + new Vector2I(0, -1)) +
                    NodeCost(node.Position + new Vector2I(0, 1)) +
                    NodeCost(node.Position + new Vector2I(-1, 1)) +
                    NodeCost(node.Position + new Vector2I(-2, 1)) +
                    NodeCost(node.Position + new Vector2I(-2, 0));
            }
            else
            {
                // node is down-right of parent.parent

                cost1 =
                    NodeCost(node.Position + new Vector2I(-2, 0)) +
                    NodeCost(node.Position + new Vector2I(-2, 1));


                cost2 =
                    NodeCost(node.Position + new Vector2I(0, -1)) +
                    NodeCost(node.Position + new Vector2I(0, 1)) +
                    NodeCost(node.Position + new Vector2I(-1, -1)) +
                    NodeCost(node.Position + new Vector2I(-2, -1)) +
                    NodeCost(node.Position + new Vector2I(-2, 0));
            }
        }


        // Case 2:
        // node is left of parent
        else if (node.Position.X + 1 == parent.Position.X &&
                node.Position.X + 2 != parent.Parent.Position.X)
        {
            if (node.Position.Y - 1 == parent.Parent.Position.Y)
            {
                // node is up-left

                cost1 =
                    NodeCost(node.Position + new Vector2I(2, 0)) +
                    NodeCost(node.Position + new Vector2I(2, -1));


                cost2 =
                    NodeCost(node.Position + new Vector2I(0, -1)) +
                    NodeCost(node.Position + new Vector2I(0, 1)) +
                    NodeCost(node.Position + new Vector2I(1, 1)) +
                    NodeCost(node.Position + new Vector2I(2, 1)) +
                    NodeCost(node.Position + new Vector2I(2, 0));
            }
            else
            {
                // node is down-left

                cost1 =
                    NodeCost(node.Position + new Vector2I(2, 0)) +
                    NodeCost(node.Position + new Vector2I(2, 1));


                cost2 =
                    NodeCost(node.Position + new Vector2I(0, -1)) +
                    NodeCost(node.Position + new Vector2I(0, 1)) +
                    NodeCost(node.Position + new Vector2I(1, -1)) +
                    NodeCost(node.Position + new Vector2I(2, -1)) +
                    NodeCost(node.Position + new Vector2I(2, 0));
            }
        }


        // Case 3:
        // node is above parent
        else if (node.Position.Y - 1 == parent.Position.Y &&
                node.Position.Y - 2 != parent.Parent.Position.Y)
        {
            if (node.Position.X - 1 == parent.Parent.Position.X)
            {
                // node is right-up

                cost1 =
                    NodeCost(node.Position + new Vector2I(0, -2)) +
                    NodeCost(node.Position + new Vector2I(-1, -2));


                cost2 =
                    NodeCost(node.Position + new Vector2I(-1, 0)) +
                    NodeCost(node.Position + new Vector2I(1, 0)) +
                    NodeCost(node.Position + new Vector2I(1, -1)) +
                    NodeCost(node.Position + new Vector2I(1, -2)) +
                    NodeCost(node.Position + new Vector2I(0, -2));
            }
            else
            {
                // node is left-up

                cost1 =
                    NodeCost(node.Position + new Vector2I(0, -2)) +
                    NodeCost(node.Position + new Vector2I(1, -2));


                cost2 =
                    NodeCost(node.Position + new Vector2I(-1, 0)) +
                    NodeCost(node.Position + new Vector2I(1, 0)) +
                    NodeCost(node.Position + new Vector2I(-1, -1)) +
                    NodeCost(node.Position + new Vector2I(-1, -2)) +
                    NodeCost(node.Position + new Vector2I(0, -2));
            }
        }


        // Case 4:
        // node is below parent
        else if (node.Position.Y + 1 == parent.Position.Y &&
                node.Position.Y + 2 != parent.Parent.Position.Y)
        {
            if (node.Position.X - 1 == parent.Parent.Position.X)
            {
                // node is right-down

                cost1 =
                    NodeCost(node.Position + new Vector2I(0, 2)) +
                    NodeCost(node.Position + new Vector2I(-1, 2));


                cost2 =
                    NodeCost(node.Position + new Vector2I(-1, 0)) +
                    NodeCost(node.Position + new Vector2I(1, 0)) +
                    NodeCost(node.Position + new Vector2I(1, 1)) +
                    NodeCost(node.Position + new Vector2I(1, 2)) +
                    NodeCost(node.Position + new Vector2I(0, 2));
            }
            else
            {
                // node is left-down

                cost1 =
                    NodeCost(node.Position + new Vector2I(0, 2)) +
                    NodeCost(node.Position + new Vector2I(1, 2));


                cost2 =
                    NodeCost(node.Position + new Vector2I(-1, 0)) +
                    NodeCost(node.Position + new Vector2I(1, 0)) +
                    NodeCost(node.Position + new Vector2I(-1, 1)) +
                    NodeCost(node.Position + new Vector2I(-1, 2)) +
                    NodeCost(node.Position + new Vector2I(0, 2));
            }
        }

        return Math.Min(cost1, cost2);
    }
}



public class PathResult
{
    public List<Tile> Path { get; } = new();

    public float Cost;

    public bool Found;
}