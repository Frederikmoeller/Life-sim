using Godot;

public partial class SceneTransition : Area2D, IInteractable
{
    [Export(PropertyHint.File, "*.tscn")] public string TargetScenePath;
    [Export] public string TargetSpawnPoint = "";
    [Export] public bool RequiresInteraction = false;
	[Export] public bool AdvanceDayTime = false;

    private bool _transitioning;


    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }


    private void OnBodyEntered(Node2D body)
    {
        if (body is not PlayerController player)
            return;


        if (!RequiresInteraction)
        {
            Interact(player);
        }
    }


    public void Interact(PlayerController player)
    {
        if (_transitioning)
            return;

        _transitioning = true;

        CallDeferred(nameof(Transition));
    }


    private void Transition()
    {
		GameManager.Instance.ChangeScene(
			TargetScenePath,
			AdvanceDayTime,
			TargetSpawnPoint
		);
    }
}