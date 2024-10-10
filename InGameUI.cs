using Godot;
using System;
using MPTest;

public partial class InGameUI : CanvasLayer
{
	[Export] private Label _testLabel;
	
	private player_new _localPawn;
	private SimpleGun _simpleGun;

	public override void _Ready()
	{
		//_localPawn = GameManager.GetLocalPlayerPawn();
		// GetNode<SceneManager>("../SceneManager").;
	}

	public override void _Process(double delta)
	{
		if (_localPawn == null) // SUX!!
		{
			_localPawn = GameManager.GetLocalPlayerPawn();
			SetupListeners();
		}

		// if (_localPawn != null) // TEST BS
		// {
		// 	_testLabel.Text = _localPawn.Name;
		// }

		if (_simpleGun != null)
		{
			ShowAmmoCount();
		}
	}

	void ShowAmmoCount()
	{
		_testLabel.Text = $"{_simpleGun.CurrentAmmo}/{_simpleGun.MaxAmmo}";
	}

	void SetupListeners()
	{
		if (_localPawn == null)
			return;

		_localPawn.GunSwitched += HandleGunSwitched;

		HandleGunSwitched();
	}

	private void HandleGunSwitched()
	{
		_simpleGun = _localPawn.Gun;
	}
}
