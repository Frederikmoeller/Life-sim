using Godot;
using System;

public partial class DebugOverlay : Control
{
    private const string VERSION_SETTING = "application/config/version";

    [Export] private Label fpsLabel;
    [Export] private Label VersionInfoLabel;

    public override void _Ready()
    {
        AddVersionToDebugInfo();
    }

    public override void _Process(double delta)
    {
        fpsLabel.Text = $"FPS: {Engine.GetFramesPerSecond()}";
    }

    private void AddVersionToDebugInfo()
    {
        String versionString = $"Version: {ProjectSettings.GetSetting(VERSION_SETTING).AsString()}";
        VersionInfoLabel.Text = versionString;
    }
}
