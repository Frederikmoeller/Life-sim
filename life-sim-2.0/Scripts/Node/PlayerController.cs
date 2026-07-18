using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerController : CharacterBody2D
{
    private bool _isMoving;
    private Level _level;
    private Dictionary<Box, BoxController> _boxControllers;

    // Sokoban Movement
    private float _moveCooldown = 0f;
    [Export] private float _moveDelay = 0.12f;

    // Exploration Movement
    [Export] private float _speed = 150f;
    private Vector2 _movementInput;

    private AnimatedSprite2D _animatedSprite;

    private Vector2I _lastDirection = Vector2I.Zero;

    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    }


    public void Initialize(Level level, Dictionary<Box, BoxController> controllers)
    {
        _level = level;
        _boxControllers = controllers;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("retry"))
        {
            if (GameManager.Instance.CurrentMode == GameMode.Sokoban)
            {    
                GameManager.Instance.Restart();
            }

            GameManager.Instance.SaveCurrentGame(1);
        }

        if (@event.IsActionPressed("Load"))
        {
            GameManager.Instance.LoadGame(1);
        }
    }

    public override void _Process(double delta)
    {

        if (GameManager.Instance.CurrentMode != GameMode.Sokoban)
            return;

        _moveCooldown -= (float)delta;

        if (_isMoving || _moveCooldown > 0f)
            return;

        HandleSokobanMovement();
    }

    public override void _PhysicsProcess(double delta)
    {
        if(GameManager.Instance.CurrentMode == GameMode.Exploration)
        {
            HandleExplorationMovement();
        }
    }

    private void Animate(Vector2I direction, bool moving)
    {
        if (direction != Vector2I.Zero)
        _lastDirection = direction;

        if (moving)
        {
            if (_lastDirection == Vector2I.Right)
            {
                _animatedSprite.Play("Walking_Sideways");
                _animatedSprite.FlipH = true;
            }
            else if (_lastDirection == Vector2I.Left)
            {
                _animatedSprite.Play("Walking_Sideways");
                _animatedSprite.FlipH = false;
            }
            else if (_lastDirection == Vector2I.Up)
            {
                _animatedSprite.Play("Walking_Up");
            }
            else if (_lastDirection == Vector2I.Down)
            {
                _animatedSprite.Play("Walking_Down");
            }
        }
        else
        {
            if (_lastDirection == Vector2I.Right)
            {
                _animatedSprite.Play("Idle_Sideways");
                _animatedSprite.FlipH = true;
            }
            else if (_lastDirection == Vector2I.Left)
            {
                _animatedSprite.Play("Idle_Sideways");
                _animatedSprite.FlipH = false;
            }
            else if (_lastDirection == Vector2I.Up)
            {
                _animatedSprite.Play("Idle_Up");
            }
            else if (_lastDirection == Vector2I.Down)
            {
                _animatedSprite.Play("Idle_Down");
            }
        }
    }

    public void TryMove(Vector2I direction)
    {
        Animate(direction, true);
        if (_level.CanMove(direction))
        {
            _level.PlayerPosition += direction;
            _moveCooldown = _moveDelay;
            MoveTo(_level.PlayerPosition);
        }
        else if (_level.CanPush(direction))
        {
            MoveBox(direction);
            _level.PlayerPosition += direction;
            _moveCooldown = _moveDelay;
            MoveTo(_level.PlayerPosition);
        }
    }

    public PlayerData GetSaveData()
    {
        return new PlayerData
        {
            PositionX = GlobalPosition.X,
            PositionY = GlobalPosition.Y
        };
    }

    private void HandleSokobanMovement()
    {
        Vector2I direction = Vector2I.Zero;

        if (Input.IsActionPressed("move_up"))
            direction = Vector2I.Up;
        else if (Input.IsActionPressed("move_down"))
            direction = Vector2I.Down;
        else if (Input.IsActionPressed("move_left"))
            direction = Vector2I.Left;
        else if (Input.IsActionPressed("move_right"))
            direction = Vector2I.Right;

        if (direction != Vector2I.Zero)
            TryMove(direction);
    }

    private void HandleExplorationMovement()
    {
        Vector2 direction = Input.GetVector(
            "move_left",
            "move_right",
            "move_up",
            "move_down"
        );

        if (direction != Vector2.Zero)
        {
            Animate((Vector2I)direction.Round(), true);
        }

        else
        {
            Animate(_lastDirection, false);
        }

        Velocity = direction * _speed;

        MoveAndSlide();
    }

    private async void MoveTo(Vector2I cell)
    {
        Vector2 target = GridUtils.GridToWorld(cell);
        _isMoving = true;

        var tween = CreateTween();
        tween.TweenProperty(this, "position", target, 0.20f);

        await ToSignal(tween, Tween.SignalName.Finished);

        _isMoving = false;
        Animate(Vector2I.Zero, false);
    }

    private async void MoveBox(Vector2I direction)
    {
        Vector2I boxPosition =
            _level.PlayerPosition + direction;


        Box box =
            _level.GetBox(boxPosition);


        if(box == null)
            return;


        Vector2I destination =
            boxPosition + direction;


        _level.MoveBox(box,destination);


        BoxController controller =
            _boxControllers[box];


        await controller.MoveTo(destination);

        GameManager.Instance.CheckCompletion();
    }

}
