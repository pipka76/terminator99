using Godot;
using System;

public partial class Aiplayer : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float SightDistance = 30.0f;
	public const float ShootDistance = 1000.0f;
	private AnimationTree _animationTree;
	private ShapeCast2D _rayCast;
	private ShapeCast2D _shootCast;
	private Vector2 _direction;
	private PackedScene _shot;
	private Marker2D _shootingPoint;
	private long _lastDelta;
	
	public override void _Ready()
	{
		_animationTree = this.GetNode<AnimationTree>("AnimationTree");
		_animationTree.Active = true;
		_animationTree.Set("parameters/playback/play", true);
		_rayCast = GetNode<ShapeCast2D>("Sight");
		_shootCast = GetNode<ShapeCast2D>("ShootCast");
		_direction = new Vector2(1, 0);
		_shot = (PackedScene)GD.Load("res://scenes/shot.tscn");
		_shootingPoint = this.GetNode<Marker2D>("ShootingPoint");

	}

	private Vector2 DecideDirection()
	{
		if (_lastDelta < DateTime.Now.AddSeconds(-1).Ticks)
		{
			TryShootEnemy();
		}

		// 0
		_rayCast.TargetPosition = _direction * SightDistance;
		_rayCast.CollisionMask = 14; // terrain objects
		if (!_rayCast.IsColliding())
		{
			return _direction;
		}
		
		// -45
		_rayCast.TargetPosition = _direction.Rotated(-(float)Math.PI / 4) * SightDistance;
		//_rayCast.CollisionMask = 1 << 2; // terrain objects
		if (!_rayCast.IsColliding())
		{
			return _direction.Rotated(-(float)Math.PI / 4);
		}
		
		// 45
		_rayCast.TargetPosition = _direction.Rotated((float)Math.PI / 4) * SightDistance;
		//_rayCast.CollisionMask = 1 << 2; // terrain objects
		if (!_rayCast.IsColliding())
		{
			return _direction.Rotated((float)Math.PI / 4);
		}
		
		var rnd = new Random();
		int newDir = rnd.Next(-314, 314);
		return _direction.Rotated(((float)newDir)/200f);
	}

	private bool TryShootEnemy()
	{
		if(!_shootCast.IsColliding())
			return false;
		
		var  collider = _shootCast.GetCollider(0);
		if (collider is Player)
		{
			ShootAt(((Node2D)collider).GlobalPosition);
			_lastDelta = DateTime.Now.Ticks;
			return true;
		}
		return false;
	}

	private void ShootAt(Vector2 globalPosition)
	{
		var shotInstance = (Node2D)_shot.Instantiate();
		var rotationAngle = Mathf.Atan2(globalPosition.Y - this.GlobalPosition.Y, globalPosition.X - this.GlobalPosition.X);

		SetShootingPoint(new Vector2(globalPosition.X - this.GlobalPosition.X,globalPosition.Y - this.GlobalPosition.Y));
		
		((Shot)shotInstance).SetShotType(1);
		shotInstance.Position = _shootingPoint.Position;
		shotInstance.Rotation = rotationAngle;
			
		_shootingPoint.AddChild(shotInstance);
	}
	
	private void SetShootingPoint(Vector2 direction)
	{
		var rotationAngle = Mathf.Atan2(direction.Y, direction.X);

		// Calculate the new position of the Marker2D relative to the player's center
		Vector2 newPosition = new Vector2(
			Mathf.Cos(rotationAngle),
			Mathf.Sin(rotationAngle)
		) * 10f;

		_shootingPoint.Position = newPosition;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;
		
		_direction = DecideDirection();
		if (_direction != Vector2.Zero)
		{
			velocity.X = _direction.X * Speed;
			velocity.Y = _direction.Y * Speed;
			
			((AnimationNodeStateMachinePlayback)_animationTree .Get("parameters/playback")).Travel("Movement");
			_animationTree.Set("parameters/Movement/blend_position", _direction);
			_animationTree.Set("parameters/Idle/blend_position", _direction);
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

	public void TakeDamage()
	{
		var hud = GetParent().GetNode<Control>("Hud");
		var pKills = hud.GetNode<RichTextLabel>("PlayerKillsValue");
		pKills.SetText((int.Parse(pKills.GetText()) + 1).ToString()); 
	}
}
