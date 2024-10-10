using Godot;
using System;

public partial class CharacterAudio : Node2D
{
	private AudioStreamPlayer2D _gunSound;
	public override void _Ready()
	{
		_gunSound = GetNode<AudioStreamPlayer2D>("GunSound");

	}

	private void PlayGunSound()
	{
		//GD.Print("BLAM");
		_gunSound.Play();
	}
}
