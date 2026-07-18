using Godot;
using System.Text.Json;

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

    private PackedScene _playerScene;
    public PlayerController CurrentPlayer { get; private set; }

    private Vector2? _targetPosition;
    private string _targetSpawnId;
    private GameScene _currentScene;

    private RandomNumberGenerator _rng;
    private SokobanGenerator generator;
    private bool levelComplete;

    public override void _Ready()
    {
        Instance = this;
        _playerScene = GD.Load<PackedScene>("res://Scenes/Player.tscn");
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

    public void ChangeScene(string scenePath, string spawnId = "", Vector2? position = null)
    {
        _targetSpawnId = spawnId;
        _targetPosition = position;

        GetTree().ChangeSceneToFile(scenePath);
    }

    public void RegisterScene(GameScene scene)
    {
        _currentScene = scene;

        SetMode(scene.Mode);

        if (CurrentMode != GameMode.Sokoban)
            SpawnPlayer(_targetSpawnId);
    }

    public void SetMode(GameMode mode)
    {
        CurrentMode = mode;
    }

    public void SpawnPlayer(string spawnId)
    {
        PlayerController player =
            _playerScene.Instantiate<PlayerController>();

        _currentScene.AddChild(player);

        CurrentPlayer = player;


        if(_targetPosition.HasValue)
        {
            player.GlobalPosition = _targetPosition.Value;
            _targetPosition = null;
        }
        else
        {
            SpawnPoint spawn = _currentScene.GetSpawnPoint(spawnId);

            if(spawn != null)
                player.GlobalPosition = spawn.GlobalPosition;
        }


        SetupPlayerCamera(player);
    }

    public void SaveCurrentGame(int slot)
    {
        SaveData data = new()
        {
            ScenePath = GetTree().CurrentScene.SceneFilePath,

            Player = CurrentPlayer.GetSaveData()
        };


        SaveManager.Instance.SaveGame(data, slot);
    }

    public void LoadGame(int slot)
    {
        SaveData data = SaveManager.Instance.LoadSaveData(slot);

        if(data == null)
            return;

        ChangeScene(
            data.ScenePath,
            position: new Vector2(
                data.Player.PositionX,
                data.Player.PositionY
            )
        );
    }

    private void SetupPlayerCamera(PlayerController player)
    {
        Camera2D camera =
            player.GetNode<Camera2D>("Camera2D");


        camera.Enabled =
            CurrentMode == GameMode.Exploration;


        if(CurrentMode != GameMode.Exploration)
            return;


        Rect2 bounds =
            _currentScene.GetCameraBounds();


        camera.LimitLeft =
            (int)bounds.Position.X;

        camera.LimitTop =
            (int)bounds.Position.Y;

        camera.LimitRight =
            (int)bounds.End.X;

        camera.LimitBottom =
            (int)bounds.End.Y;
    }

    private SpawnPoint FindSpawnPoint(string id)
    {
        if(string.IsNullOrEmpty(id))
            return null;

        foreach(Node node in _currentScene.GetTree().GetNodesInGroup("SpawnPoints"))
        {
            if(node is SpawnPoint spawn && spawn.Id == id)
                return spawn;
        }

        GD.PrintErr($"Spawn point {id} not found!");

        return null;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }
}
