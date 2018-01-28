using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Game-mode settings.
/// </summary>
public static class GameSettings
{
	/// <summary>
	/// Whether objects can be pushed out of the ring.
	/// </summary>
	public static bool RingOut = false;

	/// <summary>
	/// If true, then when capturing an unowned object, ownership *transfers* to that object,
	///     so that the player still has the same number of pieces.
	/// If false, the unowned object is just added to the player's team,
	///     increasing the size of his army.
	/// </summary>
	public static bool TransferWhenCaptureNeutral = true;

	/// <summary>
	/// The different ways to handle capturing of enemy objects.
	/// /// </summary>
	public enum EnemyCaptureModes
	{
		/// <summary>
		/// Convert the enemy object to this team.
		/// </summary>
		Convert,
		/// <summary>
		/// Transfer ownership to the enemy object, leaving the capturer neutral.
		/// </summary>
		Transfer,
		/// <summary>
		/// Destroy the enemy object.
		/// </summary>
		Destroy,
	}
	public static EnemyCaptureModes EnemyCaptureMode = EnemyCaptureModes.Transfer;

	/// <summary>
	/// The number of players in the game.
	/// </summary>
	public static int NPlayers = 0;
	/// <summary>
	/// The number of objects to spawn.
	/// </summary>
	public static int NObjectsInField = 40;
	/// <summary>
	/// The percentage of objects that belong to a player in the beginning.
	/// </summary>
	public static float NObjectsPerPlayerPercent = 0.25f;
	/// <summary>
	/// Affects powerup spawn rates.
	/// </summary>
	public static float PowerupFrequency = 1.0f;
	
	/// <summary>
	/// Different ways to control the game.
	/// </summary>
	public enum ControlSchemes
	{
		WASD = 0,
		ArrowKeys,
		IJKL,

		Gamepad1, Gamepad2, Gamepad3, Gamepad4,

		N_SCHEMES
	}
	/// <summary>
	/// The player control schemes.
	/// </summary>
	public static List<ControlSchemes> PlayerControlSchemes = new List<ControlSchemes>()
	{
		ControlSchemes.WASD,
		ControlSchemes.ArrowKeys,
	};
}