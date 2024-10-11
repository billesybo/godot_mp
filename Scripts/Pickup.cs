using Godot;
using System;
using MPTest;

public partial class Pickup : Area2D
{
	private bool _isPickedUp = false;
	
	void OnBodyEntered(Node2D node)
	{
		if (_isPickedUp)
			return;
		
		if (node.IsInGroup("Player"))
		{
			_isPickedUp = true;
			((player_new)node).SwitchGun(GunKind.AssaultRifle);
			QueueFree();
			
			// TODO audio before queuefree? Or let player do it??
		}
	}
}

