using Godot;
using System;

public partial class Shot : Area2D
{
	private const float SPEED = 1000;
	private const float RANGE = 1000;
	private float _travelledDistance = 0;

	public void SetShotType(int level)
	{
		var sprite = this.GetNode<Sprite2D>(level.ToString("00"));
		if (sprite != null)
			sprite.Visible = true;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var direction = Vector2.Right.Rotated(this.Rotation);
		this.Position += direction * SPEED * (float)delta;
		_travelledDistance += SPEED * (float)delta;
		if(_travelledDistance > RANGE)
			QueueFree();
	}

	public void _on_body_entered(GodotObject body)
	{
		QueueFree();
		if (body.HasMethod("TakeDamage"))
		{
			GD.Print("Damage!");
			body.Call("TakeDamage");
		}
	}
}
