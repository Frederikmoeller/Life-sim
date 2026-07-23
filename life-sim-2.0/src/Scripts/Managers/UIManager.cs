using Godot;
using System.Collections.Generic;

public partial class UIManager : Node
{
    public static UIManager Instance { get; private set; }

    private Control _hudRoot;
    private Control _pauseRoot;
    private Control _transitionRoot;
    private Control _debugRoot;

    private readonly Stack<Control> _hudStack = new();
    private readonly Stack<Control> _pauseStack = new();
    private readonly Stack<Control> _transitionStack = new();
    private readonly Stack<Control> _debugStack = new();

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
    }

    public void SetRoots(Control hudRoot, Control pauseRoot, Control transitionRoot, Control debugRoot)
    {
        _hudRoot = hudRoot;
        _pauseRoot = pauseRoot;
        _transitionRoot = transitionRoot;
        _debugRoot = debugRoot;
    }

    public void PushHud(PackedScene scene) => PushScreen(scene, _hudRoot, _hudStack);
    public void PushPause(PackedScene scene) => PushScreen(scene, _pauseRoot, _pauseStack);
    public void PushTransition(PackedScene scene) => PushScreen(scene, _transitionRoot, _transitionStack);
    public void PushDebug(PackedScene scene) => PushScreen(scene, _debugRoot, _debugStack);

    public void PopHud() => PopScreen(_hudStack);
    public void PopPause() => PopScreen(_pauseStack);
    public void PopTransition() => PopScreen(_transitionStack);
    public void PopDebug() => PopScreen(_debugStack);

    public void ClearHud() => ClearStack(_hudStack);
    public void ClearPause() => ClearStack(_pauseStack);
    public void ClearTransition() => ClearStack(_transitionStack);
    public void ClearDebug() => ClearStack(_debugStack);

    public void ShowMainMenu()
    {
        ClearHud();
        PushHud(GD.Load<PackedScene>("res://src/Scenes/UI/Menus/MainMenu.tscn"));
    }

    private void PushScreen(PackedScene scene, Control root, Stack<Control> stack)
    {
        if (root == null || scene == null)
            return;

        if (stack.Count > 0)
        {
            Control current = stack.Peek();
            if (current != null && IsInstanceValid(current))
                current.Visible = false;
        }

        Control ui = scene.Instantiate<Control>();
        root.AddChild(ui);
        stack.Push(ui);
    }

    private void PopScreen(Stack<Control> stack)
    {
        if (stack.Count == 0)
            return;

        Control top = stack.Pop();
        if (top != null && IsInstanceValid(top))
            top.QueueFree();

        if (stack.Count > 0)
        {
            Control next = stack.Peek();
            if (next != null && IsInstanceValid(next))
                next.Visible = true;
        }
    }

    private void ClearStack(Stack<Control> stack)
    {
        while (stack.Count > 0)
        {
            Control ui = stack.Pop();
            if (ui != null && IsInstanceValid(ui))
                ui.QueueFree();
        }
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }
}