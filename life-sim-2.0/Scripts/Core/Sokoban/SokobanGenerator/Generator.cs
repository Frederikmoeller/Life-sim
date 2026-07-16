using Godot;
using System.Collections.Generic;


public class Generator
{
    public static void GeneratePaths(Level level)
    {
        int steps = 0;
        int safety = 0;

        List<Box> originalBoxes = CopyBoxes(level, true);
        List<Box> ghostBoxes =
            CopyBoxes(level, false);


        while(level.SolveCounter > 0)
        {
                safety++;

                if(safety > 100)
                {
                    GD.Print("Generator failed after 100 pushes");
                    level.Trash = true;
                    return;
                }


            List<PathResult> boxPaths =
                CalculateBoxPaths(level, ghostBoxes);


            PlayerPathResult playerPaths =
                CalculatePlayerPaths(
                    level,
                    ghostBoxes,
                    boxPaths
                );


            int bestPath =
                playerPaths.BestPathIndex;


            if(bestPath == -1)
            {
                GD.Print("No solution");
                level.Trash = true;
                return;
            }

            // Get selected paths
            PathResult playerPath =
                playerPaths.Paths[bestPath];

            PathResult boxPath =
                boxPaths[bestPath];


            // Remove walls from player path
            foreach(Tile tile in playerPath.Path)
            {
                tile.Wall = false;

                if(tile.Occupied)
                {
                    level.Trash = true;
                    GD.Print("Trash level: player path hit box");
                }
            }

            int stop = 0;
            // Move ghost box one push
            Box thisBox = ghostBoxes[bestPath];

            Vector2I currentNode = boxPath.Path[0].Position;

            Vector2I diff = currentNode - thisBox.Position;

            // Continue along straight part of box path
            if(boxPath.Path.Count > 1)
            {
                for(int i = 1; i < boxPath.Path.Count; i++)
                {
                    Vector2I nextNode =
                        boxPath.Path[i].Position;


                    Vector2I nextDiff =
                        nextNode - currentNode;


                    if(nextDiff == diff)
                    {
                        currentNode = nextNode;
                    }
                    else
                    {
                        stop = i - 1;
                        break;
                    }
                }
            }
            
            // Remove walls from box path
            for(int i = 0; i <= stop; i++)
            {
                boxPath.Path[i].Wall = false;
            }

            // Move ghost box
            Vector2I oldPosition = thisBox.Position;

            Vector2I newPosition =
                boxPath.Path[stop].Position;


            level.Grid[oldPosition.X, oldPosition.Y].Occupied = false;
            level.Grid[oldPosition.X, oldPosition.Y].Wall = false;

            thisBox.SetPosition(newPosition);

            level.Grid[newPosition.X, newPosition.Y].Occupied = true;
            level.PlayerPosition = newPosition - diff;


            // Move player behind box after push
            level.PlayerPosition = playerPath.Path[playerPath.Path.Count - 1].Position;

            if(thisBox.Position == thisBox.Goal.Position)
            {
                thisBox.Placed = true;

                level.Grid[
                    thisBox.Position.X,
                    thisBox.Position.Y
                ].Occupied = false;


                level.SolveCounter--;

                ghostBoxes.RemoveAt(bestPath);
            }
        }

        level.PlayerPosition = level.PlayerStartPosition;
        level.Boxes = originalBoxes;
    }
    
    public static List<Box> CopyBoxes(Level level, bool used)
    {
        List<Box> newBoxes = new();


        foreach(Box box in level.Boxes)
        {
            Box copy = new Box(
                box.Position,
                box.Goal
            );

            newBoxes.Add(copy);


            Tile tile = level.Grid[
                box.Position.X,
                box.Position.Y
            ];


            tile.Occupied = true;
            tile.ContainsBox = true;
            tile.Used = used;
        }


        return newBoxes;
    }

    public static List<PathResult> CalculateBoxPaths(Level level, List<Box> ghostBoxes)
    {
        List<PathResult> boxPaths = new();


        foreach(Box box in ghostBoxes)
        {
            Tile boxTile = level.Grid[
                box.Position.X,
                box.Position.Y
            ];


            // temporarily remove box
            boxTile.Occupied = false;


            Pathfinder solver = new Pathfinder(
                level,
                box.Position,
                box.Goal.Position
            );


            PathResult result = solver.FindPath(true);

            if(!result.Found)
            {
                GD.PrintErr("Box cannot reach goal: ", box.Position);

                // restore box before exiting
                boxTile.Occupied = true;

                return null;
            }

            boxPaths.Add(result);

            // restore box
            boxTile.Occupied = true;
        }


        return boxPaths;
    }

    public static PlayerPathResult CalculatePlayerPaths(Level level, List<Box> ghostBoxes, List<PathResult> boxPaths)
    {
        PlayerPathResult result = new();

        float lowestCost = float.MaxValue;


        for(int i = 0; i < ghostBoxes.Count; i++)
        {
            Box box = ghostBoxes[i];

            Vector2I firstPush =
                boxPaths[i].Path[0].Position;


            Vector2I playerTarget = box.Position;


            if(firstPush.X == box.Position.X + 1)
            {
                playerTarget.X -= 1;
            }
            else if(firstPush.X == box.Position.X - 1)
            {
                playerTarget.X += 1;
            }
            else if(firstPush.Y == box.Position.Y + 1)
            {
                playerTarget.Y -= 1;
            }
            else
            {
                playerTarget.Y += 1;
            }


            Pathfinder solver = new Pathfinder(
                level,
                level.PlayerPosition,
                playerTarget
            );


            PathResult playerPath =
                solver.FindPath(false);


            result.Paths.Add(playerPath);

            if(!playerPath.Found || playerPath.Path.Count == 0)
            {
                result.BestPathIndex = -1;
                return result;
            }


            if(playerPath.Cost < lowestCost)
            {
                lowestCost = playerPath.Cost;
                result.BestPathIndex = i;
            }
        }


        return result;
    }
}

public class PlayerPathResult
{
    public List<PathResult> Paths = new();

    public int BestPathIndex = -1;
}