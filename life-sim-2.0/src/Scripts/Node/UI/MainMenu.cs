using Godot;
using System;

public partial class MainMenu : Control
{
    [Export] private VBoxContainer ButtonList;

    public override void _Ready()
    {
        BuildMenu();
    }

    private void BuildMenu()
    {
        foreach (Node child in ButtonList.GetChildren())
            child.QueueFree();

        if (SaveManager.Instance != null && SaveManager.Instance.HasSave(1))
            AddMenuButton("Continue", OnContinuePressed, 0);

        AddMenuButton("Start Game", OnStartGamePressed);
        AddMenuButton("Quit", OnQuitPressed);
    }

    private void AddMenuButton(string text, Action callback, int index = -1)
    {
        Button button = new Button();
        button.Text = text;
        button.Pressed += callback;

        ButtonList.AddChild(button);

        if (index >= 0 && index < ButtonList.GetChildCount() - 1)
            ButtonList.MoveChild(button, index);
    }

    private void OnContinuePressed()
    {
        UIManager.Instance.ClearHud();
        GameManager.Instance.LoadGame(1);
    }

    private void OnStartGamePressed()
    {
        UIManager.Instance.ClearHud();
        GameManager.Instance.StartGame();
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }
}