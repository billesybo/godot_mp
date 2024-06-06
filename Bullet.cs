using Godot;
using MPTest;

public partial class Bullet : Area2D
{
	public const float Speed = 500.0f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	private Vector2 _direction;
	private bool _hitSomething;
	
	public override void _Ready()
	{
		_direction = Vector2.Right.Rotated(Rotation);

		_hitSomething = false;
	}

	public override void _PhysicsProcess(double delta)
	{
		Position += _direction * (float)delta * Speed;

	}

	void _on_timer_timeout()
	{
		QueueFree();
	}

	// TODO this should maybe run on server only, and health should be synced. 
	void _on_body_entered(Node2D node)
	{
		if (_hitSomething)
			return;
		_hitSomething = true;
		
		if (node.IsInGroup("Player"))
		{
			((player_new)node).DoDamage();
		}

		if (node.IsInGroup("Enemies"))
		{
			((enemy)node).DoDamage();
		}

		QueueFree();
	}
}



