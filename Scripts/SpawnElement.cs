using Godot;
using System;

public partial class SpawnElement : Node2D
{
	// [Export] private Area2D _area;

	[Export] private CollisionShape2D _shape;

	public Vector2 Size =>  _shape.Shape.GetRect().Size; // PEH!!


	// // Called when the node enters the scene tree for the first time.
	// public override void _Ready()
	// {
	// 	//Size = _shape.Shape.GetRect().Size;
	// }

}
