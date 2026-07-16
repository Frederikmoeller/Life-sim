using Godot;
using System;

public enum GameMode
{
    Exploration,
    Sokoban
}

public partial class GameManager : Node
{
    [Signal] public delegate void LevelCompletedEventHandler();
    public static GameManager Instance { get; private set; }
    public GameMode CurrentMode { get; private set; }
    public Level CurrentLevel { get; private set; }

    private RandomNumberGenerator _rng;

    private SokobanGenerator generator;
    private bool levelComplete;

    public override void _Ready()
    {
        Instance = this;
        _rng = new RandomNumberGenerator();
    }

    public void RegisterGenerator(SokobanGenerator node)
    {
        generator = node;
    }

    public void StartNewGame()
    {
        levelComplete = false;

        CurrentLevel = generator.GenerateLevel(_rng.RandiRange(10, 20), _rng.RandiRange(3, 7));
    }

    public void Restart()
    {
        levelComplete = false;

        CurrentLevel.Reset();

        generator.ResetLevel(CurrentLevel);
    }

    public void CheckCompletion()
    {
        if(levelComplete)
        return;

        if(CurrentLevel.IsSolved())
        {
            levelComplete = true;
            EmitSignal(SignalName.LevelCompleted);
            GD.Print("Level Completed!");
        }
    }

    public void ChangeScene(string scenePath)
    {
        GetTree().ChangeSceneToFile(scenePath);
    }

    public void SetMode(GameMode mode)
    {
        CurrentMode = mode;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }
}
