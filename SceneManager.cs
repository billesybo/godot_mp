using Godot;
using System;
using System.Diagnostics;
using Godot.Collections;
using MPTest;

public partial class SceneManager : Node2D
{
	[Export] private PackedScene _playerScene;

	[Export] private PackedScene _enemyScene;

	private const int WaveSize = 2;
	private const float WaveIntervalMin = 3f;
	private const float WaveIntervalMax = 7f;
	private Timer _enemySpawnTimer;

	private int _enemySpawnIndex = 0;

	private const int EnemyMax = 5;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		int index = 0;
		foreach (PlayerInfo playerInfo in GameManager.Players)
		{
			player_new spawnedPlayer = _playerScene.Instantiate<player_new>();
			spawnedPlayer.Name = playerInfo.Id.ToString();
			AddChild(spawnedPlayer);
			spawnedPlayer.ShowName(playerInfo.Name);
			
			foreach (Node2D spawnPoint in GetTree().GetNodesInGroup("SpawnPoints"))
			{
				if (int.Parse(spawnPoint.Name) == index)
				{
					spawnedPlayer.GlobalPosition = spawnPoint.GlobalPosition;
				}
			}

			index++;
		}
		
		InitEnemySpawn();
	}

	void InitEnemySpawn()
	{
		_enemySpawnTimer = GetNode<Timer>("EnemySpawnTimer");
		SetEnemySpawnTime();
		_enemySpawnTimer.Timeout += SpawnNextWave;
		_enemySpawnTimer.Start();
	}

	void SetEnemySpawnTime()
	{
		_enemySpawnTimer.WaitTime = GD.RandRange(WaveIntervalMin, WaveIntervalMax);
	}

	void SpawnNextWave()
	{
		if (Multiplayer.IsServer())
		{
			Array<Node> spawnPoints = GetTree().GetNodesInGroup("SpawnPoints");
			int count = Mathf.Min(WaveSize, spawnPoints.Count);
			for (int i = 0; i < count; i++)
			{
				if (GameManager.NumEnemies >= EnemyMax)
					break;

				_enemySpawnIndex %= spawnPoints.Count;
				Rpc("SpawnEnemy", ((Node2D)spawnPoints[_enemySpawnIndex]).GlobalPosition);
				_enemySpawnIndex++;
			}
		}
		SetEnemySpawnTime();
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, CallLocal = true)]
	void SpawnEnemy(Vector2 position)
	{
		enemy spawnedEnemy = _enemyScene.Instantiate<enemy>();
		AddChild(spawnedEnemy);

		spawnedEnemy.GlobalPosition = position;
	}

}
