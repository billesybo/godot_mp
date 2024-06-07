using Godot;
using System;
using MPTest;

public partial class enemy : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float JumpVelocity = -400.0f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	private int _health = 3;

	private Label _label;
	private Timer _talkTimer;
	private Timer _actionTimer;

	private Vector2 _direction = Vector2.Right;
	private bool _jumpNextFrame;

	public override void _Ready()
	{
		base._Ready();

		_label = GetNode<Label>("Label");
		_talkTimer = GetNode<Timer>("Label/Timer");
		_actionTimer = GetNode<Timer>("ActionTimer");
		
		_actionTimer.Start();
		_actionTimer.Timeout += PickAction;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y += gravity * (float)delta;

		// // Handle Jump.
		if (_jumpNextFrame && IsOnFloor())
		{
			_jumpNextFrame = false;
			velocity.Y = JumpVelocity;
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = _direction;
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
	}
	public void DoDamage()
	{
		ShowPainText();

		_health--;

		if (_health <= 0)
		{
			// This should probably be RPCd
			// Cleanup();
			// QueueFree();
			Rpc("RemoveEnemyRPC");
		}
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	void RemoveEnemyRPC()
	{
		GD.Print("RUNNING REMOVE!!");
		Cleanup();
		QueueFree();
	}

	void ShowPainText()
	{
		Say(TextCollection.GetRandomOuch());	
	}

	void Say(string tosay)
	{
		_label.Text = tosay;
		_talkTimer.Start();
		_talkTimer.Timeout += HandleSayTimeout;
	}

	private void HandleSayTimeout()
	{
		_label.Text = String.Empty;
		_talkTimer.Timeout -= HandleSayTimeout;
	}

	void Cleanup()
	{
		_talkTimer.Timeout -= HandleSayTimeout;
	}

	void PickAction()
	{
		if (GD.RandRange(0, 1) == 0)
		{
			_direction = Vector2.Right;
		}
		else
		{
			_direction = Vector2.Left;
		}

		if (GD.Randf() < 0.5f)
		{
			_jumpNextFrame = true;
		}
	}
}
