using Godot;
using System;
using System.Collections.Generic;
using MPTest;

public partial class GameManager : Node
{
	// [Signal]
	// public delegate void SceneManagerInitializedEventHandler();

	
	public static List<PlayerInfo> Players = new List<PlayerInfo>();

	public static int NumEnemies;

	
	// SUX! IMPROVE THESE
	public static player_new GetLocalPlayerPawn()
	{
		foreach (PlayerInfo info in Players)
		{
			if (info.PlayerPawn != null && info.PlayerPawn.IsLocalOwned)
			{
				return info.PlayerPawn;
			}
		}

		return null;
	}

	public static PlayerInfo GetLocalPlayerInfo() 
	{
		foreach (PlayerInfo info in Players)
		{
			if (info.PlayerPawn != null && info.PlayerPawn.IsLocalOwned)
			{
				return info;
			}
		}

		return null;
	}

	public static void AddScore(int id, int score)
	{
		foreach (PlayerInfo info in Players)
		{
			if (info.Id == id)
			{
				info.Score += score;
				return;
			}
		}
	}

	// public static void NotifySceneManagerInitialized()
	// {
	// 	EmitSignal(SignalName.SceneManagerInitialized);
	// }


}
