using Godot;

namespace MPTest;

public partial class player_new : CharacterBody2D
{
	[Export] 
	public PackedScene Bullet;

	Camera2D _camera;

	public const float Speed = 300.0f;
	public const float JumpVelocity = -500.0f;

	private Vector2 _syncPosition;
	private float _syncRotation;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	// Cached stuff
	private Node2D _gunRotation;
	private AnimatedSprite2D _animatedSprite;
	
	public override void _Ready()
	{
		GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetMultiplayerAuthority(int.Parse(Name)); // TODO CACHE
		_gunRotation = GetNode<Node2D>("GunRotation");
		_camera = GetNode<Camera2D>("Camera2D");
		_animatedSprite = GetNode<AnimatedSprite2D> ("AnimatedSprite2D");
	}

	public override void _PhysicsProcess(double delta)
	{
		// Remote pawn (wow structure this code, asshole!)
		if (GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority() !=
			Multiplayer.GetUniqueId()) 
		{
			GlobalPosition = GlobalPosition.Lerp(_syncPosition, 0.1f); 
			_gunRotation.RotationDegrees = Mathf.Lerp(_gunRotation.RotationDegrees, _syncRotation, 0.1f);
			_camera.Enabled = false;
			
			//GD.Print($"_syncPosition - GlobalPosition {_syncPosition - GlobalPosition}");
			UpdateAnimations(_syncPosition - GlobalPosition);
			
			return;
		}
		
		// Local pawn!!
		Vector2 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y += gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_up") && IsOnFloor())
			velocity.Y = JumpVelocity;

		var inverse = _camera.GetCanvasTransform().AffineInverse();
		var mousePosWorld = inverse * _camera.GetViewport().GetMousePosition();
		_gunRotation.LookAt(mousePosWorld);
		
		if (Input.IsActionJustPressed("fire"))
		{
			Rpc("FireRPC");
		}

		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
		
		_syncPosition = GlobalPosition;
		_syncRotation = _gunRotation.RotationDegrees;
		
		_camera.Enabled = true;
		
		UpdateAnimations(direction);
	}

	void UpdateAnimations(Vector2 direction)
	{
		if(direction.X != 0)
			_animatedSprite.FlipH = direction.X > 0;

		//if (!IsOnFloor())
		if (Velocity.Y < 0)		
		{
			_animatedSprite.Play("falling");
			return;
		}

//		if(Mathf.IsEqualApprox(direction.X, 0))
		if(PrettyMuchZero(direction.X))
		{
			_animatedSprite.Play("default");
		}
		else
		{
			_animatedSprite.Play("run");
		}
	}

	bool PrettyMuchZero(float number)
	{
		return number < 0.004 && number > -0.004;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	void FireRPC()
	{
		Node2D bullet = Bullet.Instantiate<Node2D>();
		bullet.RotationDegrees = _gunRotation.RotationDegrees;
		bullet.GlobalPosition = GetNode<Node2D>("GunRotation/BulletSpawn").GlobalPosition;
		GetTree().Root.AddChild(bullet);
	}

	public void ShowName(string name)
	{
		GetNode<Label>("Label").Text = name;
	}
}
