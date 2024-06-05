using Godot;
using System;
using MPTest;

public partial class SceneManager : Node2D
{
	[Export] private PackedScene _playerScene;
	
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
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
