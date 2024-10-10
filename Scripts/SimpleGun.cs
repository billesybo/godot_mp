using System.Reflection.Metadata;
using Godot;

public partial class SimpleGun : Node2D
{
	[Export] 
	public PackedScene Bullet;

	[Export]
	private AudioStreamPlayer2D _fireSound;

	[Export] 
	private Timer _fireTimer;

	[Export] 
	private Timer _reloadTimer;
	
	[Export]
	private int _maxAmmo = 5;

	[Export] 
	private bool _autoFire;
	
	public int MaxAmmo => _maxAmmo;

	public int CurrentAmmo { get; private set; }


	private Node2D _bulletSpawn;
	
	public override void _Ready()
	{
		_bulletSpawn = GetNode<Node2D>("BulletSpawn");
		_fireTimer.Stop();
		_reloadTimer.Timeout += HandleReload;

		HandleReload();
	}

	private void HandleReload()
	{
		CurrentAmmo = MaxAmmo;
		_reloadTimer.Stop();
	}

	public void TryFireGun()
	{
		if (CurrentAmmo <= 0)
		{
			return;
			// TODO CLICK SOUND 
		}

		if (_reloadTimer.TimeLeft > 0) // no shooting while reloading!
		{
			return;
		}

		if (_fireTimer.TimeLeft > 0)
		{
			//GD.Print($"time left {_fireTimer.TimeLeft}");
			return;
		}

		_fireTimer.Start();
		CurrentAmmo--;
		Rpc("FireRPC");
	}

	public void TryAutoFire()
	{
		if(_autoFire)
			TryFireGun();
	}

	public void TryReload()
	{
		GD.Print($"MEH reload ze gun");
		Rpc("ReloadRPC");
	}


	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)] 
	void FireRPC()
	{
		GD.Print($"MEH shoot ze gun");
		
		Node2D bullet = Bullet.Instantiate<Node2D>();
		bullet.RotationDegrees = _bulletSpawn.GlobalRotationDegrees; // RotationDegrees; //_rotationDegrees;
		// bullet.GlobalPosition = GetNode<Node2D>("GunRotation/BulletSpawn").GlobalPosition;
		bullet.GlobalPosition = _bulletSpawn.GlobalPosition;
		GetTree().Root.AddChild(bullet);
		
		//EmitSignal(SignalName.GunFired);
		_fireSound.Play();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	void ReloadRPC()
	{
		GD.Print($"MEH shoot ze gun");
		_reloadTimer.Start();
	}


}

public enum GunKind
{
	None = 0,
	Pistol = 1,
	AssaultRifle = 2,
}
