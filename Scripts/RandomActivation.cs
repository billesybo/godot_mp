using Godot;
using System;
using Godot.Collections;
using Range = Godot.Range;

public partial class RandomActivation : Node2D
{
	[Export] 
	private float _activationChance;

	[Export] private PackedScene _spawned;

	public override void _Ready()
	{
		if (Multiplayer.IsServer())
		{
			bool activate = GD.Randf() < _activationChance;
			//Rpc("SetEnabledRPC", activate);
			if (activate)
			{
				GD.Print("LE SPAWNZ");
				Area2D spawned = _spawned.Instantiate<Area2D>();
				AddChild(spawned);
				spawned.Position = Vector2.Zero;
			}
		}
	}

	// [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	// void SetEnabledRPC(bool enabled)
	// {
	// 	Array<Node> children = GetChildren();
	// 	foreach (Node child in children)
	// 	{
	// 		(child as Node2D).
	// 	}
	// }



}
