using Godot;
using System.Text.Json;

public enum GameMode
{
    Exploration,
    Sokoban
}
public enum DayTime
{
    Morning,
    Midday,
    Evening,
    Night
}

public partial class GameManager : Node
{

    [Signal] public delegate void LevelCompletedEventHandler();

    public static GameManager Instance { get; private set; }

    public GameMode CurrentMode { get; private set; }
    public Level CurrentLevel { get; private set; }
    public DayTime CurrentDayTime { get; private set; }
    public PlayerController CurrentPlayer { get; private set; }

    private PackedScene _playerScene;
    private PackedScene _currentSceneScene;

    private Vector2? _targetPosition;
    private string _targetSpawnId;
    private GameScene _currentScene;

    private Node2D _worldRoot;
    private Node _levelRoot;

    private RandomNumberGenerator _rng;
    private SokobanGenerator generator;
    private bool levelComplete;


    public override void _Ready()
    {
        Instance = this;
        _playerScene = GD.Load<PackedScene>("res://src/Scenes/Player.tscn");
        _rng = new RandomNumberGenerator();
        ProcessMode = ProcessModeEnum.Always;
    }

    public void StartGame()
    {
        ChangeScene("res://src/Scenes/Levels/players_room.tscn");
    }

    public void SetSceneRoots(Node2D worldRoot, Node levelRoot)
    {
        _worldRoot = worldRoot;
        _levelRoot = levelRoot;
    }

    public void RegisterGenerator(SokobanGenerator node)
    {
        generator = node;
    }

    public void StartNewSokobanGame()
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

    public void ChangeScene(string scenePath, bool advanceDayTime = false, string spawnId = "", Vector2? position = null)
    {
        _targetSpawnId = spawnId;
        _targetPosition = position;

        if (advanceDayTime)
            AdvanceDayTime();

        CallDeferred(nameof(LoadSceneDeferred), scenePath);
    }

    private void LoadSceneDeferred(string scenePath)
    {
        if (_currentScene != null && IsInstanceValid(_currentScene))
        {
            _currentScene.QueueFree();
            _currentScene = null;
        }

        if (_currentSceneScene != null)
        {
            _currentSceneScene = null;
        }

        PackedScene packedScene = GD.Load<PackedScene>(scenePath);
        if (packedScene == null)
        {
            GD.PushError($"Failed to load scene: {scenePath}");
            return;
        }

        Node instance = packedScene.Instantiate();

        if (instance is not GameScene gameScene)
        {
            GD.PushError($"Scene is not a GameScene: {scenePath}");
            instance.QueueFree();
            return;
        }

        _currentSceneScene = packedScene;
        _currentScene = gameScene;

        _levelRoot.AddChild(gameScene);
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
        PlayerController player = _playerScene.Instantiate<PlayerController>();
        _currentScene.AddChild(player);
        CurrentPlayer = player;

        if (_targetPosition.HasValue)
        {
            player.GlobalPosition = _targetPosition.Value;
            _targetPosition = null;
        }
        else
        {
            SpawnPoint spawn = null;

            if (!string.IsNullOrEmpty(spawnId))
                spawn = _currentScene.GetSpawnPoint(spawnId);

            if (spawn == null)
            {
                string defaultSpawnId = _currentScene.GetDefaultSpawnPointId();
                if (!string.IsNullOrEmpty(defaultSpawnId))
                    spawn = _currentScene.GetSpawnPoint(defaultSpawnId);
            }

            if (spawn != null)
                player.GlobalPosition = spawn.GlobalPosition;
        }

        SetupPlayerCamera(player);
    }

    public void SaveGame(int slot)
    {
        if (_currentScene == null)
        return;

        SaveData data = new()
        {
            CurrentDayTime = GetDayTime(),
            ScenePath = _currentScene?.SceneFilePath ?? "",
            Player = CurrentPlayer.GetSaveData(),
            World = WorldStateManager.Instance?.GetSaveData() ?? new WorldData(),
            Inventory = InventoryManager.Instance?.GetSaveData() ?? new InventoryData()
        };

        SaveManager.Instance.SaveGame(data, slot);
    }

    public void LoadGame(int slot)
    {
        SaveData data = SaveManager.Instance.LoadSaveData(slot);

        if(data == null)
            return;

        WorldStateManager.Instance?.Load(data.World);
        InventoryManager.Instance?.Load(data.Inventory);

        SetDayTime(data.CurrentDayTime);

        ChangeScene(
            data.ScenePath,
            false,
            position: new Vector2(
                data.Player.PositionX,
                data.Player.PositionY
            )
        );
    }

    public void SetDayTime(DayTime newDayTime) 
    {
        CurrentDayTime = newDayTime;
    }

    public DayTime GetDayTime()
    {
        return CurrentDayTime;
    }

    public DayTime GetNextDayTime(DayTime time)
    {
        return time switch
        {
            DayTime.Morning => DayTime.Midday,
            DayTime.Midday => DayTime.Evening,
            DayTime.Evening => DayTime.Night,
            DayTime.Night => DayTime.Morning,
            _ => DayTime.Morning
        };
    }

    public void AdvanceDayTime()
    {
        CurrentDayTime = GetNextDayTime(CurrentDayTime);
    }


    private void SetupPlayerCamera(PlayerController player)
    {
        Camera2D camera = player.GetNode<Camera2D>("Camera2D");

        camera.Enabled = CurrentMode == GameMode.Exploration;

        if (CurrentMode != GameMode.Exploration)
            return;

        Rect2 bounds = _currentScene.GetCameraBounds();

        camera.LimitLeft = (int)bounds.Position.X;
        camera.LimitTop = (int)bounds.Position.Y;
        camera.LimitRight = (int)bounds.End.X;
        camera.LimitBottom = (int)bounds.End.Y;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }
}
