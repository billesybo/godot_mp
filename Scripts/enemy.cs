using Godot;
using System;
using System.Diagnostics;
using MPTest;

public partial class enemy : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float JumpVelocity = -800.0f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	private float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	private int _health = 3;

	private Label _label;
	private Timer _talkTimer;
	private Timer _actionTimer;

	private bool _jumpNextFrame;
	
	private AnimatedSprite2D _animatedSprite;

	private bool _isAttacking;
	
	// SYNCED
	private Vector2 _direction = Vector2.Right;
	private Vector2 _syncPosition;


	public override void _Ready()
	{
		base._Ready();

		_label = GetNode<Label>("Label");
		_talkTimer = GetNode<Timer>("Label/Timer");
		_actionTimer = GetNode<Timer>("ActionTimer");
		_animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		
		_actionTimer.Start();
		_actionTimer.Timeout += PickAction;
		
		UpdateAnimation();
		
		_talkTimer.Timeout += HandleSayTimeout;
		
		// THIS IS SHIT
		GameManager.NumEnemies++;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!Multiplayer.IsServer())
		{
			//GD.Print($"Client pos updating to {_syncPosition}");
			GlobalPosition = GlobalPosition.Lerp(_syncPosition, 0.1f); 
			return;
		}

		Vector2 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y += _gravity * (float)delta;

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

		_animatedSprite.FlipH = velocity.X < 0;

		Velocity = velocity;
		MoveAndSlide();

		// Set the synced position for teh syncing
		// GD.Print($"Server pos updating to {_syncPosition}");
		_syncPosition = GlobalPosition;
	}

	void UpdateAnimation()
	{
		if (_isAttacking)
		{
			_animatedSprite.Play("attack");
		}
		else
		{
			_animatedSprite.Play("run");
		}
	}

	public void DoDamage(int ownerId)
	{
		ShowPainText();

		_health--;

		if (_health <= 0)
		{
			Rpc("RemoveEnemyRPC");
		}
		
		GameManager.AddScore(ownerId, 10);
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	void RemoveEnemyRPC()
	{
		Cleanup();
		QueueFree();
	}

	void ShowPainText()
	{
		Say(TextCollection.GetRandomOuch());	
	}

	void Say(string toSay, float chance = 1f) // TODO move all the stupid talking logic to its own class
	{
		// if (_talkTimer.IsStopped())
		// 	return;
		
		_label.Text = toSay;
		_talkTimer.Start();
		// _talkTimer.Timeout -= HandleSayTimeout;
		// _talkTimer.Timeout += HandleSayTimeout;
	}

	private void HandleSayTimeout()
	{
		_label.Text = String.Empty;
		_talkTimer.Stop();
		//_talkTimer.Timeout -= HandleSayTimeout;
	}

	void Cleanup()
	{
		// THIS IS SHIT
		GameManager.NumEnemies--;

		//_talkTimer.
		_talkTimer.Timeout -= HandleSayTimeout;
	}

	void PickAction()
	{
		if (!Multiplayer.IsServer()) // Server picks actions, actions sync
			return;
		
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

	void Attack(Node2D node)
	{
		if(_isAttacking)
			return;
		
		_isAttacking = true;
		_animatedSprite.Play("attack");
		_animatedSprite.AnimationFinished += AttackFinished;
		UpdateAnimation();
		((player_new)node).DoDamage(-1);
	}

	private void AttackFinished()
	{
		_animatedSprite.AnimationFinished -= AttackFinished;
		_isAttacking = false;
		UpdateAnimation();
	}
}


