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

	public override void _Ready()
	{
		AddSpawnableScene(_spawned.ResourcePath);
		
		if (Multiplayer.IsServer())
		{
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
