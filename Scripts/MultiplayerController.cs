using Godot;
using System.Collections.Generic;
using System.Linq;
using MPTest;

public partial class MultiplayerController : Control
{
	private const string TestScenePath = "res://Scenes/TestScene.tscn";
	private const string RandomScenePath = "res://Scenes/RandomScene.tscn";
	
	[Export]
	private int _port = 28910;

	[Export] 
	private string _address = "127.0.0.1";

	[Export] 
	private ENetConnection.CompressionMode _compressionMode = ENetConnection.CompressionMode.RangeCoder;

	ENetMultiplayerPeer _peer;

	private LineEdit _ipLineEdit;

	private Label _labelLog;
	private const int _maxLogSize = 10; 
	private List<string> _log = new List<string>(_maxLogSize);

	private Button _hostButton;
	private Button _startButton;
	private Button _joinButton;
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Multiplayer.PeerConnected += HandlePeerConnected;
		Multiplayer.PeerDisconnected += HandlePeerDisconnected;
		Multiplayer.ConnectedToServer += HandleConnectedToServer;
		Multiplayer.ConnectionFailed += HandleConnectionFailed;

		_hostButton = GetNode<Button>("Button_Host");
		_startButton = GetNode<Button>("Button_Start");
		_joinButton = GetNode<Button>("Button_Join");

		GetNode<LineEdit>("LineEdit").Text = TextCollection.GetRandomName(); // TODO Cache
		_ipLineEdit = GetNode<LineEdit>("LineEdit_ip");
		_ipLineEdit.Text = _address;

		_labelLog = GetNode<Label>("Label_log");
		UpdateLog();

		_startButton.Disabled = true;
	}

	// Only client
	private void HandleConnectionFailed()
	{
		AddToLog("CONNECTION FAILED");
		_joinButton.Disabled = false;
	}

	// Only client
	private void HandleConnectedToServer()
	{
		AddToLog("CONNECTED TO SERVER");
		RpcId(1,"SendPlayerInfoRPC", GetNode<LineEdit>("LineEdit").Text, Multiplayer.GetUniqueId());
	}

	// Runs on all
	private void HandlePeerDisconnected(long id)
	{
		AddToLog($"PEER DISCONNECTED {id}");
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
		AddToLog($"PEER CONNECTED {id}");
	}

	public override void _Process(double delta)
	{
	}
	
	public void _on_button_host_button_down()
	{
		AddToLog("HOST");
		_peer = new ENetMultiplayerPeer();
		Error error = _peer.CreateServer(_port, maxClients: 4);

		if (error != Error.Ok)
		{
			AddToLog($"ERROR HOSTING {error.ToString()}");
			return;
		}
		
		_peer.Host.Compress(_compressionMode);
		Multiplayer.MultiplayerPeer = _peer;
		AddToLog("Waiting for players");
		// SendPlayerInfoRPC(GetNode<LineEdit>("LineEdit").Text, Multiplayer.GetUniqueId());
		SendPlayerInfoRPC(GetNode<LineEdit>("LineEdit").Text, 1);

		_hostButton.Disabled = true;
		_joinButton.Disabled = true;
		_startButton.Disabled = false;
	}
	
	public void _on_button_join_button_down()
	{
		AddToLog("JOIN");
		_peer = new ENetMultiplayerPeer();
		
		Error error = _peer.CreateClient(_ipLineEdit.Text, _port);

		if (error != Error.Ok)
		{
			AddToLog("Failed to join!!");
			return;
		}

		_peer.Host.Compress(_compressionMode);
		Multiplayer.MultiplayerPeer = _peer;
		AddToLog("Joining game");
		_joinButton.Disabled = true;
		_hostButton.Disabled = true;
	}
	
	public void _on_button_start_button_down()
	{
		AddToLog("START GAME");

		Rpc("LoadStartSceneRPC");
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	void LoadStartSceneRPC()
	{
		foreach (PlayerInfo player in GameManager.Players)
		{
			AddToLog($"Player: {player.Name} id : {player.Id}");
		}
		
		Node2D scene = ResourceLoader.Load<PackedScene>(RandomScenePath).Instantiate<Node2D>();
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

	void AddToLog(string entry)
	{
		GD.Print(entry);
		_log.Add(entry);
		UpdateLog();
	}

	void UpdateLog()
	{
		while (_log.Count > _maxLogSize)
		{
			_log.RemoveAt(_log.Count - 1);
		}

		string text = string.Empty;
		foreach (string entry in _log)
		{
			text += $"{entry}\n";
		}

		_labelLog.Text = text;
	}
}



