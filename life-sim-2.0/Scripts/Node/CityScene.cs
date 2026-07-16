using Godot;

public partial class CityScene : Node2D
{
    public override void _Ready()
    {
        GameManager.Instance.SetMode(GameMode.Exploration);
    }
}