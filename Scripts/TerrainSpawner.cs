using Godot;
using System;

public partial class TerrainSpawner : MultiplayerSpawner//Node2D
{
	[Export] private PackedScene[] _spawnElements;

	[Export] private Node2D _startPoint;

	private Vector2 _currentPosition;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		foreach (var scene in _spawnElements)
		{
			AddSpawnableScene(scene.ResourcePath);
		}
		
		GD.Print($"READY {Multiplayer.IsServer()}");
		if (Multiplayer.IsServer())
		{
			_currentPosition = _startPoint.GlobalPosition;

			for (int i = 0; i < 5; i++)
			{
				PackedScene element = GetRandomElement();
				//SpawnElement(element);
				//Rpc("SpawnElementRPC", element);
				SpawnElementRPC(element, i);
			}
		}
	}

	//[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	void SpawnElementRPC(PackedScene element, int index)
	{
		GD.Print($"SPAWNZING {Multiplayer.IsServer()} {_currentPosition} {element.ResourceName}");
		
		SpawnElement spawned = element.Instantiate<SpawnElement>();
		spawned.Name += index.ToString();
		_currentPosition += Vector2.Right * spawned.Size.X / 2;
		//AddChild(spawned);
		_startPoint.AddChild(spawned);
		spawned.GlobalPosition = _currentPosition;
		_currentPosition += Vector2.Right * spawned.Size.X / 2;
	}

	PackedScene GetRandomElement()
	{
		int index = GD.RandRange(0, _spawnElements.Length - 1);
		GD.Print($"spawning index {index}");
		return _spawnElements[index];
	}
}
