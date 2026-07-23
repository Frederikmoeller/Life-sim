using Godot;
using System.Collections.Generic;

public partial class InteractionDetector : Area2D
{
    private List<IInteractable> _nearby = new();


    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
        AreaExited += OnAreaExited;
    }


    public override void _UnhandledInput(InputEvent @event)
    {
        if(@event.IsActionPressed("interact"))
        {
            TryInteract();
        }
    }


    private void TryInteract()
    {
        GD.Print("Pressed");
        if(_nearby.Count == 0)
            return;

        GD.Print("Pressed nearby interactables");

        _nearby[0].Interact(
            GetParent<PlayerController>()
        );
    }


    private void OnAreaEntered(Node2D area)
    {
        if(area is IInteractable interactable)
        {
            _nearby.Add(interactable);
        }
    }


    private void OnAreaExited(Node2D area)
    {
        if(area is IInteractable interactable)
        {
            _nearby.Remove(interactable);
        }
    }
}