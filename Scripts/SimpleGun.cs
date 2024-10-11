using System.Reflection.Metadata;
using Godot;

public partial class SimpleGun : Node2D
{
	[Export] 
	public PackedScene Bullet;

	[Export]
	private AudioStreamPlayer2D _fireSound;

	[Export]
	private AudioStreamPlayer2D _reloadSound;

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
	private Sprite2D _gunSprite;
	
	public override void _Ready()
	{
		_bulletSpawn = GetNode<Node2D>("BulletSpawn");
		_fireTimer.Stop();
		_reloadTimer.Timeout += HandleReload;

		_gunSprite = GetNode<Sprite2D>("Sprite2D");

		HandleReload();
	}

	public override void _PhysicsProcess(double delta)
	{
		//GD.Print($"GlobalRotation {GlobalRotation}");

		double absRotation = Mathf.Abs(GlobalRotation);
		bool flipped = absRotation > Mathf.Pi / 2;

		_gunSprite.FlipV = flipped;
	}

	private void HandleReload()
	{
		CurrentAmmo = MaxAmmo;
		_reloadTimer.Stop();
	}

	public void TryFireGun(int multiplayerAuthority)
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
		Rpc("FireRPC", multiplayerAuthority);
	}

	public void TryAutoFire(int multiplayerAuthority)
	{
		if(_autoFire)
			TryFireGun(multiplayerAuthority);
	}

	public void TryReload()
	{
		Rpc("ReloadRPC");
		_reloadSound.Play();
	}


	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)] 
	void FireRPC(int multiplayerAuthority)
	{
		GD.Print($"MEH shoot ze gun. Server {Multiplayer.IsServer()} {multiplayerAuthority}");
		
		Bullet bullet = Bullet.Instantiate<Bullet>();
		bullet.RotationDegrees = _bulletSpawn.GlobalRotationDegrees; // RotationDegrees; //_rotationDegrees;
		// bullet.GlobalPosition = GetNode<Node2D>("GunRotation/BulletSpawn").GlobalPosition;
		bullet.GlobalPosition = _bulletSpawn.GlobalPosition;
		bullet.OwnerId = multiplayerAuthority;
		
		GetTree().Root.AddChild(bullet);
		
		//EmitSignal(SignalName.GunFired);
		_fireSound.Play();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	void ReloadRPC()
	{
		GD.Print($"MEH reload ze gun. Server {Multiplayer.IsServer()}");
		_reloadTimer.Start();
	}


}

public enum GunKind
{
	None = 0,
	Pistol = 1,
	AssaultRifle = 2,
}
