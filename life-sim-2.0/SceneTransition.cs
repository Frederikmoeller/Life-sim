using Godot;
using System;

public partial class SceneTransition : Area2D
{
    [Export(PropertyHint.File, "*.tscn")] public string TargetScenePath;
    [Export] public string TargetSpawnPoint = "";
	[Export] public bool RequiresInteraction = false;

	private bool _transitioning;


    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

	private void OnBodyEntered(Node2D body)
	{
		if (_transitioning)
        return;

		if (body is not PlayerController)
			return;

		_transitioning = true;

		CallDeferred(nameof(Transition));
	}

	private void Transition()
	{
		GameManager.Instance.ChangeScene(
			TargetScenePath,
			TargetSpawnPoint
		);
	}
}
