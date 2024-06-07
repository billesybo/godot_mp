using Godot;

namespace MPTest;

public partial class player_new : CharacterBody2D
{
	[Export] 
	public PackedScene Bullet;

	Camera2D _camera;

	public const float Speed = 300.0f;
	public const float JumpVelocity = -600.0f;

	private Vector2 _syncPosition;
	private float _syncRotation;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	// Cached stuff
	private Node2D _gunRotation;
	private AnimatedSprite2D _animatedSprite;

	private const int StartHealth = 3;
	private int _health;

	private bool _dead;

	private Timer _respawnTimer;
	
	public override void _Ready()
	{
		GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetMultiplayerAuthority(int.Parse(Name)); // TODO CACHE
		_gunRotation = GetNode<Node2D>("GunRotation");
		_camera = GetNode<Camera2D>("Camera2D");
		_animatedSprite = GetNode<AnimatedSprite2D> ("AnimatedSprite2D");
		_respawnTimer = GetNode<Timer>("RespawnTimer");

		_health = StartHealth;
		UpdateHealthVisuals();

		Input.MouseMode = Input.MouseModeEnum.Confined;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_dead)
			return;
		
		// Remote pawn (wow structure this code, asshole!)
		if (GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority() !=
			Multiplayer.GetUniqueId()) 
		{
			GlobalPosition = GlobalPosition.Lerp(_syncPosition, 0.1f); 
			_gunRotation.RotationDegrees = Mathf.Lerp(_gunRotation.RotationDegrees, _syncRotation, 0.1f);
			_camera.Enabled = false;
			
			//GD.Print($"_syncPosition - GlobalPosition {_syncPosition - GlobalPosition}");
			Vector2 remoteMove = _syncPosition - GlobalPosition;
			UpdateAnimations(remoteMove, remoteMove);
			
			return;
		}
		
		// Local pawn!!
		Vector2 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y += gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("up") && IsOnFloor())
			velocity.Y = JumpVelocity;

		var inverse = _camera.GetCanvasTransform().AffineInverse();
		var mousePosWorld = inverse * _camera.GetViewport().GetMousePosition();
		_gunRotation.LookAt(mousePosWorld);
		
		if (Input.IsActionJustPressed("fire"))
		{
			Rpc("FireRPC");
		}

		Vector2 direction = Input.GetVector("left", "right", "up", "down");
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
		
		UpdateAnimations(direction, Velocity);
	}

	void UpdateAnimations(Vector2 direction, Vector2 velocity)
	{
		if(direction.X != 0)
			_animatedSprite.FlipH = direction.X > 0;

		if (!PrettyMuchZero(velocity.Y))		
		{
			_animatedSprite.Play("falling");
			return;
		}

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

	public void DoDamage()
	{
		if (_dead)
			return;
		
		_health--;

		UpdateHealthVisuals();

		if (_health <= 0)
		{
			_dead = true;
			_animatedSprite.Play("death");
			_respawnTimer.Start();
			_respawnTimer.Timeout += Respawn;
		}
	}

	void UpdateHealthVisuals()
	{
		var node = GetNode<Node2D>("HealthIndicator");
		int count = node.GetChildCount();
		for (int i = 0; i < count; i++)
		{
			((Sprite2D)node.GetChild(i)).Visible = _health > i;
		}
	}

	void Respawn()
	{
		_respawnTimer.Timeout -= Respawn;
		_respawnTimer.Stop();
		
		_health = StartHealth;
		_dead = false;
		Vector2 newPos = GlobalPosition;
		newPos += Vector2.Up *700;
		GlobalPosition = newPos;
		
		UpdateHealthVisuals();
	}
}
