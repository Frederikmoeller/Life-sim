using Godot;

public partial class GameScene : Node2D
{
    [Export] public GameMode Mode;

    [Export] public TileMapLayer WorldTileMap;

    [Export] public NodePath SpawnPointsPath;

    public override void _Ready()
    {
        GameManager.Instance.RegisterScene(this);
        GameManager.Instance.SetMode(Mode);
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
