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
	public static bool RingOut = true;

	/// <summary>
	/// Different ways to handle an object "capturing" another.
	/// </summary>
	public enum CaptureModes
	{
		Convert,
		Transfer,
		Destroy,
	}
	public static CaptureModes CaptureMode;

	/// <summary>
	/// The number of players in the game.
	/// </summary>
	public static int NPlayers = 2;
	/// <summary>
	/// The number of objects to spawn.
	/// </summary>
	public static int NObjectsInField = 30,
					  NObjectsPerPlayer = 4;
	
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