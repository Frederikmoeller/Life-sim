using Godot;

public partial class GameScene : Node2D
{
    [Export] public GameMode Mode;

    [Export] public TileMapLayer WorldTileMap;

    [Export] public NodePath SpawnPointsPath;
    [Export] public NodePath DefaultSpawnPoint;

    [Export] public Light2D WorldLight;
    [Export] private Node Lights;
    private Godot.Collections.Array<Node> PointLights;

    private DayTime _currentDayTime;

    public override void _Ready()
    {
        GameManager.Instance.RegisterScene(this);
        GameManager.Instance.SetMode(Mode);
        _currentDayTime = GameManager.Instance.GetDayTime();

        PointLights = Lights.GetChildren();
        

        switch (_currentDayTime)
        {
            case DayTime.Morning:
                foreach (Node2D item in PointLights)
                {
                    item.Visible = false;
                }
                WorldLight.Color = new Color(1f, 0.9f, 0.8f);
                WorldLight.Energy = 0.8f;
                break;

            case DayTime.Midday:
                foreach (Node2D item in PointLights)
                {
                    item.Visible = false;
                }
                WorldLight.Color = new Color(1f, 1f, 1f);
                WorldLight.Energy = 0.0f;
                break;

            case DayTime.Evening:
                foreach (Node2D item in PointLights)
                {
                    item.Visible = false;
                }
                WorldLight.Color = new Color(0.8f, 0.63f, 0.57f);
                WorldLight.Energy = 0.6f;
                break;

            case DayTime.Night:
                foreach (Node2D item in PointLights)
                {
                    item.Visible = true;
                }
                WorldLight.Color = new Color(0f, 0.27f, 0.74f);
                WorldLight.Energy = 0.85f;
                break;

            default:
                break;
        }
    }

    public SpawnPoint GetSpawnPoint(string id)
    {
        var spawnRoot = GetNode<Node>(SpawnPointsPath);

        foreach (Node child in spawnRoot.GetChildren())
        {
            if (child is SpawnPoint spawn && spawn.Id == id)
                return spawn;
        }

        return null;
    }

    public string GetDefaultSpawnPointId()
    {
        if (DefaultSpawnPoint == null || DefaultSpawnPoint.IsEmpty)
            return null;

        SpawnPoint spawn = GetNodeOrNull<SpawnPoint>(DefaultSpawnPoint);
        return spawn?.Id;
    }

    public virtual Rect2 GetCameraBounds()
    {
        Rect2 usedRect = WorldTileMap.GetUsedRect();

        Vector2 tileSize =
            WorldTileMap.TileSet.TileSize;


        return new Rect2(
            usedRect.Position * tileSize,
            usedRect.Size * tileSize
        );
    }
}
