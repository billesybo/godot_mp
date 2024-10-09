using Godot;
using System;

public partial class SimpleGun : Node2D
{
	[Export] 
	public PackedScene Bullet;

	private Node2D _bulletSpawn;
	
	public override void _Ready()
	{
		_bulletSpawn = GetNode<Node2D>("BulletSpawn");
		
	}

	public bool TryFireGun()
	{
		Rpc("FireRPC");
		return true;
	}


	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)] 
	void FireRPC()
	{
		Node2D bullet = Bullet.Instantiate<Node2D>();
		bullet.RotationDegrees = _bulletSpawn.GlobalRotationDegrees; // RotationDegrees; //_rotationDegrees;
		// bullet.GlobalPosition = GetNode<Node2D>("GunRotation/BulletSpawn").GlobalPosition;
		bullet.GlobalPosition = _bulletSpawn.GlobalPosition;
		GetTree().Root.AddChild(bullet);
		
		//EmitSignal(SignalName.GunFired); TODO SIGNAAAAL
	}

}
