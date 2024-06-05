using Godot;
using System;

namespace MPTest;

public partial class player_new : CharacterBody2D
{
	[Export] 
	public PackedScene Bullet;

	Camera2D _camera;

	public const float Speed = 300.0f;
	public const float JumpVelocity = -400.0f;

	private Vector2 _syncPosition;
	private float _syncRotation;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	// Cached stuff
	private Node2D _gunRotation;
	public override void _Ready()
	{
		GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetMultiplayerAuthority(int.Parse(Name)); // TODO CACHE
		_gunRotation = GetNode<Node2D>("GunRotation");
		_camera = GetNode<Camera2D>("Camera2D");
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
			return;
		}
		
		// Local pawn!!
		Vector2 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y += gravity * (float)delta;

		// Handle Jump.
		// if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		// 	velocity.Y = JumpVelocity;
		if (Input.IsActionJustPressed("ui_up") && IsOnFloor())
			velocity.Y = JumpVelocity;

		//_gunRotation.LookAt(_camera.GetViewport().GetMousePosition());

		//GD.Print($"Camera {_camera.GetViewport().GetMousePosition()}");
		//GD.Print($"Viewport {GetViewport().GetMousePosition()}");
		//GD.Print($"Player pos : {Position} mouse pos {_camera.GetViewport().GetMousePosition()} diff {_camera.GetViewport().GetMousePosition() - Position}");

		var inverse = _camera.GetCanvasTransform().AffineInverse();
		var mousePosWorld = inverse * _camera.GetViewport().GetMousePosition();
		//GD.Print($"Player pos : {Position} mouse pos world {mousePosWorld} diff {mousePosWorld - Position}");
		
		Vector2 playerToMouse = mousePosWorld - Position;
		//_gunRotation.LookAt(Position + playerToMouse);
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
