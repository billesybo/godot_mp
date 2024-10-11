using Godot;
using System;

public partial class GameEnder : Node
{
	private const string EndScenePath = "res://Scenes/EndScene.tscn";
	private bool _ending;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GameManager.Done = false;
		_ending = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// if (Multiplayer.IsServer())
		// {
		// 	// THIS TOTALLY BLOWS!!
		// 	if (!_ending && GameManager.Done)
		// 	{
		// 		_ending = true;
		// 		Rpc("EndGameRPC");
		// 	}
		// }
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	void EndGameRPC()
	{
		// BOO SUX DOESN'T WORK RIGHT!
		
		GetTree().ChangeSceneToPacked(ResourceLoader.Load<PackedScene>(EndScenePath));

		// Node meh = GetTree().Root.GetNode("RandomScene");
		// GetTree().Root.RemoveChild(meh);
		// meh.CallDeferred("QueueFree");
		//
		// Node scene = ResourceLoader.Load<PackedScene>(EndScenePath).Instantiate<Node>();
		// // GetTree().CurrentScene.QueueFree();
		// //GetTree().CurrentScene = scene;
		// GetTree().Root.AddChild(scene);
		// GetTree().ch
	}
}
