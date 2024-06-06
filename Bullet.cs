using Godot;

public partial class Bullet : CharacterBody2D
{
	public const float Speed = 500.0f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	private Vector2 _direction;
	public override void _Ready()
	{
		_direction = Vector2.Right.Rotated(Rotation);
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		velocity = Speed * _direction;
		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y += gravity * (float)delta;


		Velocity = velocity;
		MoveAndSlide();
	}

	void _on_timer_timeout()
	{
		QueueFree();
	}
}

