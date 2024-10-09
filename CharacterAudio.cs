using Godot;
using System;

public partial class CharacterAudio : Node2D
{
	private AudioStreamPlayer2D _gunSound;
	public override void _Ready()
	{
		_gunSound = GetNode<AudioStreamPlayer2D>("GunSound");

	}

	public void PlayGunSound()
	{
		_gunSound.Play();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
