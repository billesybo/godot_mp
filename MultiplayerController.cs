using Godot;
using System;
using System.Linq;
using MPTest;

public partial class MultiplayerController : Control
{
	[Export]
	private int _port = 28910;

	[Export] 
	private string _address = "127.0.0.1";

	[Export] 
	private ENetConnection.CompressionMode _compressionMode = ENetConnection.CompressionMode.RangeCoder;

	ENetMultiplayerPeer _peer;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Multiplayer.PeerConnected += HandlePeerConnected;
		Multiplayer.PeerDisconnected += HandlePeerDisconnected;
		Multiplayer.ConnectedToServer += HandleConnectedToServer;
		Multiplayer.ConnectionFailed += HandleConnectionFailed;
	}

	// Only client
	private void HandleConnectionFailed()
	{
		GD.Print("CONNECTION FAILED");
	}

	// Only client
	private void HandleConnectedToServer()
	{
		GD.Print("CONNECTED TO SERVER");
		RpcId(1,"SendPlayerInfoRPC", GetNode<LineEdit>("LineEdit").Text, Multiplayer.GetUniqueId());
	}

	// Runs on all
	private void HandlePeerDisconnected(long id)
	{
		GD.Print($"PEER DISCONNECTED {id}");
		GameManager.Players.Remove((PlayerInfo)GameManager.Players.Where(i => i.Id == id).First());
		foreach (player_new player in GetTree().GetNodesInGroup("Player"))
		{
			if(int.Parse(player.Name) == id)
			{
				player.QueueFree();
			}
		}

	}

	// Runs on all
	private void HandlePeerConnected(long id)
	{
		GD.Print($"PEER CONNECTED {id}");
	}

	public override void _Process(double delta)
	{
	}
	
	public void _on_button_host_button_down()
	{
		GD.Print("HOST");
		_peer = new ENetMultiplayerPeer();
		Error error = _peer.CreateServer(_port, maxClients: 4);

		if (error != Error.Ok)
		{
			GD.Print($"ERROR HOSTING {error.ToString()}");
			return;
		}
		
		_peer.Host.Compress(_compressionMode);
		Multiplayer.MultiplayerPeer = _peer;
		GD.Print("Waiting for players");
		// SendPlayerInfoRPC(GetNode<LineEdit>("LineEdit").Text, Multiplayer.GetUniqueId());
		SendPlayerInfoRPC(GetNode<LineEdit>("LineEdit").Text, 1);
	}
	
	public void _on_button_join_button_down()
	{
		GD.Print("JOIN");
		_peer = new ENetMultiplayerPeer();
		Error error = _peer.CreateClient(_address, _port);

		// if(error != Error.Ok)
		
		_peer.Host.Compress(_compressionMode);
		Multiplayer.MultiplayerPeer = _peer;
		GD.Print("Joining game");
	}

	public void _on_button_start_button_down()
	{
		GD.Print("START GAME");

		Rpc("LoadStartSceneRPC");
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	void LoadStartSceneRPC()
	{
		foreach (PlayerInfo player in GameManager.Players)
		{
			GD.Print($"Player: {player.Name} id : {player.Id}");
		}
		
		Node2D scene = ResourceLoader.Load<PackedScene>("res://TestScene.tscn").Instantiate<Node2D>();
		GetTree().Root.AddChild(scene);
		this.Hide(); // hide UI (queue free??)
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	void SendPlayerInfoRPC(string name, int id)
	{
		var info = new PlayerInfo()
		{
			Name = name,
			Id = id,
		};

		bool inList = false;
		foreach (PlayerInfo playerInfo in GameManager.Players)
		{
			if (playerInfo.Id == id)
			{
				inList = true;
				break;
			}
		}
		if (!inList)
		{
			GameManager.Players.Add(info);
		}

		if (Multiplayer.IsServer())
		{
			foreach (PlayerInfo playerInfo in GameManager.Players)
			{
				Rpc("SendPlayerInfoRPC", playerInfo.Name, playerInfo.Id);
			}
		}
		
	}
}



