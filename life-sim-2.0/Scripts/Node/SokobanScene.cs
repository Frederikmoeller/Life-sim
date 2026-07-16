using Godot;

public partial class SokobanScene : Node2D
{
    private Camera2D camera;

    public override void _Ready()
    {
        camera = GetNode<Camera2D>("Camera2D");
        SokobanGenerator generator = GetNode<SokobanGenerator>("SokobanGenerator");

        GameManager.Instance.RegisterGenerator(generator);
        GameManager.Instance.SetMode(GameMode.Sokoban);
        GameManager.Instance.StartNewGame();
        GameManager.Instance.LevelCompleted += OnLevelCompleted;
        camera.Enabled = true;
        CenterCamera();
        FitCamera();
    }

    private void CenterCamera()
    {
        Level level = GameManager.Instance.CurrentLevel;

        Vector2 center = new Vector2(
            level.Width * 32 / 2f,
            level.Height * 32 / 2f
        );

        camera.Position = center;
    }

    private void FitCamera()
    {
        Level level = GameManager.Instance.CurrentLevel;

        float boardWidth = level.Width * 32;
        float boardHeight = level.Height * 32;

        float viewSize = 500f;

        float zoom = viewSize / Mathf.Max(
            boardWidth,
            boardHeight
        );


        camera.Zoom = new Vector2(zoom, zoom);
    }

    private async void OnLevelCompleted()
    {
        await ToSignal(
            GetTree().CreateTimer(1.0),
            Timer.SignalName.Timeout
        );

        GameManager.Instance.ChangeScene("res://Scenes/City.tscn");
    }

    public override void _ExitTree()
    {
        GameManager.Instance.LevelCompleted -= OnLevelCompleted;
    }
}