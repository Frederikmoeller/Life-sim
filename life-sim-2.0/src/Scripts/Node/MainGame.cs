using Godot;

public partial class MainGame : Node
{
    [Export] private Node2D World;
    [Export] private Node LevelRoot;
    [Export] private Control HudRoot;
    [Export] private Control PauseRoot;
    [Export] private Control TransitionRoot;
    [Export] private Control DebugRoot;
    public override void _Ready()
    {
        GameManager.Instance.SetSceneRoots(World, LevelRoot);
        UIManager.Instance.SetRoots(HudRoot, PauseRoot, TransitionRoot, DebugRoot);
        UIManager.Instance.ShowMainMenu();
    }
}