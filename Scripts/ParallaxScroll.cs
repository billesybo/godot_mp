using Godot;
using System;

public partial class ParallaxScroll : ParallaxLayer
{
	[Export] 
	private float _speed = -15;

	public override void _Process(double delta)
	{
		MotionOffset = new Vector2(MotionOffset.X + _speed * (float)delta, MotionOffset.Y);
	}
}
