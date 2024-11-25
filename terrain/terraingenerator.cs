using Godot;
using System;

public partial class terraingenerator : Node
{
	private Vector2 _cellSize = new Vector2(16, 16);
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var screen = GetWindow().Size;
//		int xTiles = _window.size.x/_cellSize.x;
//		int yTiles = _window.size.y/_cellSize.y;
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
