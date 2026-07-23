using Godot;
using System.Collections.Generic;

public partial class PathfinderTest : Node
{
    public override void _Ready()
    {
        Level level = new Level(20, 20, 1);
        // Make floor
        for(int x = 1; x < 19; x++)
        {
            for(int y = 1; y < 19; y++)
            {
                level.Grid[x,y].Wall = false;
            }
        }

        // Make a tiny starting room
        level.Grid[2,3].Wall = false;
        level.Grid[4,3].Wall = false;


        // Place box
        Vector2I boxPos = new Vector2I(5,3);

        level.Grid[boxPos.X, boxPos.Y].ContainsBox = true;
        level.Grid[boxPos.X, boxPos.Y].Occupied = true;


        Goal goal = new Goal(new Vector2I(17,5));

        Box box = new Box(boxPos, goal);

        level.Boxes.Add(box);


        // Place player
        level.PlayerPosition = new Vector2I(3,3);
        level.PlayerStartPosition = level.PlayerPosition;

        // THIS IS WHERE IT GOES
        Generator.GeneratePaths(level);
    }
}