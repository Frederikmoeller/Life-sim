using Godot;
using System.Threading.Tasks;

public partial class BoxController : Node2D
{
    private Box _box;

    private const int TileSize = 32;


    public void Initialize(Box box)
    {
        _box = box;

        Position = GridUtils.GridToWorld(box.Position);
    }


    public async Task MoveTo(Vector2I cell)
    {
        Vector2 target = GridUtils.GridToWorld(cell);

        Tween tween = CreateTween();

        tween.TweenProperty(
            this,
            "position",
            target,
            0.12f
        );

        await ToSignal(
            tween,
            Tween.SignalName.Finished
        );
    }
}