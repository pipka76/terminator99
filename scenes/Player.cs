using Godot;
using System;

public partial class Player : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float JumpVelocity = -400.0f;
	private AnimationTree _animationTree;
	private PackedScene _shot;
	private Marker2D _shootingPoint;
	private Vector2 _lookingDirection;
	
	public override void _Ready()
	{
		_shot = (PackedScene)GD.Load("res://scenes/shot.tscn");
		_animationTree = this.GetNode<AnimationTree>("AnimationTree");
		_animationTree.Active = true;
		_animationTree.Set("parameters/playback/play", true);
		_shootingPoint = this.GetNode<Marker2D>("ShootingPoint");
		_lookingDirection = Vector2.Right;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;
		
		// Handle fire
		if (Input.IsActionJustPressed("ui_accept"))
		{
			var shotInstance = (Node2D)_shot.Instantiate();
			var rotationAngle = Mathf.Atan2(_lookingDirection.Y, _lookingDirection.X);

			((Shot)shotInstance).SetShotType(2);
			shotInstance.Position = _shootingPoint.Position;
			shotInstance.Rotation = rotationAngle;
			
			_shootingPoint.AddChild(shotInstance);
		}
		
		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		if (direction != Vector2.Zero)
		{
			_lookingDirection = direction;
			velocity.X = direction.X * Speed;
			velocity.Y = direction.Y * Speed;

			((AnimationNodeStateMachinePlayback)_animationTree .Get("parameters/playback")).Travel("Movement");
			_animationTree.Set("parameters/Movement/blend_position", direction);
			_animationTree.Set("parameters/Idle/blend_position", direction);
			
			SetShootingPoint(direction);
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Y = Mathf.MoveToward(Velocity.Y, 0, Speed);
			
			((AnimationNodeStateMachinePlayback)_animationTree .Get("parameters/playback")).Travel("Idle");
		}

		Velocity = velocity;
		
		MoveAndSlide();
	}

	private void SetShootingPoint(Vector2 direction)
	{
		var rotationAngle = Mathf.Atan2(direction.Y, direction.X);

		// Calculate the new position of the Marker2D relative to the player's center
		Vector2 newPosition = new Vector2(
			Mathf.Cos(rotationAngle),
			Mathf.Sin(rotationAngle)
		) * 8.5f;

		_shootingPoint.Position = newPosition;
	}
	
	public void TakeDamage()
	{
		var hud = GetParent().GetNode<Control>("Hud");
		var pKills = hud.GetNode<RichTextLabel>("AiKillsValue");
		pKills.SetText((int.Parse(pKills.GetText()) + 1).ToString()); 
	}
}
