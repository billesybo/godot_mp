using Godot;
using System;
using Godot.Collections;
using Range = Godot.Range;

public partial class RandomActivation : MultiplayerSpawner
{
	[Export] 
	private float _activationChance;

	[Export] private PackedScene _spawned;

	[Export] private Node2D _spawnRoot;

	[Export] private Timer _timer;

	[Export] private float _minTime;
	[Export] private float _maxTime;
	

	public override void _Ready()
	{
		AddSpawnableScene(_spawned.ResourcePath);

		StartTimer();
		
		AttemptSpawn();
	}

	void StartTimer()
	{
		_timer.WaitTime = _minTime + GD.Randf() * (_maxTime - _minTime);
		_timer.Start();
		_timer.Timeout += AttemptSpawn;
	}

	public void AttemptSpawn()
	{
		if (Multiplayer.IsServer())
		{
			if (_spawnRoot.GetChildren().Count > 0)
				return;
			
			bool activate = GD.Randf() < _activationChance;
			//Rpc("SetEnabledRPC", activate);
			if (activate)
			{
				GD.Print("LE SPAWNZ");
				Area2D spawned = _spawned.Instantiate<Area2D>();
				_spawnRoot.AddChild(spawned);
				spawned.GlobalPosition = _spawnRoot.GlobalPosition;
			}
		}	
	}

}
