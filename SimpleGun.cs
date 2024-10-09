using Godot;
using System;

public partial class SimpleGun : Node2D
{
	[Export] 
	public PackedScene Bullet;

	[Export]
	private AudioStreamPlayer2D _fireSound;

	[Export] 
	private Timer _fireTimer;

	private Node2D _bulletSpawn;
	
	public override void _Ready()
	{
		_bulletSpawn = GetNode<Node2D>("BulletSpawn");
		_fireTimer.Stop();
	}

	public void TryFireGun()
	{
		if (_fireTimer.TimeLeft > 0)
		{
			//GD.Print($"time left {_fireTimer.TimeLeft}");
			return;
		}

		_fireTimer.Start();
		Rpc("FireRPC");
	}


	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)] 
	void FireRPC()
	{
		Node2D bullet = Bullet.Instantiate<Node2D>();
		bullet.RotationDegrees = _bulletSpawn.GlobalRotationDegrees; // RotationDegrees; //_rotationDegrees;
		// bullet.GlobalPosition = GetNode<Node2D>("GunRotation/BulletSpawn").GlobalPosition;
		bullet.GlobalPosition = _bulletSpawn.GlobalPosition;
		GetTree().Root.AddChild(bullet);
		
		//EmitSignal(SignalName.GunFired);
		_fireSound.Play();
	}

}
