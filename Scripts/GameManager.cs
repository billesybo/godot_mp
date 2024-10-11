using Godot;
using System.Collections.Generic;
using MPTest;

public partial class GameManager : Node
{
	public static List<PlayerInfo> Players = new List<PlayerInfo>();

	public static int NumEnemies;

	public static bool Done;
	
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

				CheckGameOver(info.Score);
				return;
			}
		}
	}

	static void CheckGameOver(int score)
	{
		if (score > 10)
		{
			Done = true;
		}
	}

}
