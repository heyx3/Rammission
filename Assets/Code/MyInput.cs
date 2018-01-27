using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class MyInput : MonoBehaviour
{
	public static Vector2 GetInput(int playerID) { return playerInputs[playerID]; }
	private static List<Vector2> playerInputs = new List<Vector2>();


	private void Update()
	{
		for (int i = 0; i < GameSettings.NPlayers; ++i)
		{
			if (playerInputs.Count <= i)
				playerInputs.Add(new Vector2());

			switch (GameSettings.PlayerControlSchemes[i])
			{
				case GameSettings.ControlSchemes.WASD:
					playerInputs[i] = new Vector2((Input.GetKey(KeyCode.W) ? 1 : 0) -
												      (Input.GetKey(KeyCode.S) ? 1 : 0),
												  (Input.GetKey(KeyCode.D) ? 1 : 0) -
													  (Input.GetKey(KeyCode.A) ? 1 : 0));
					break;
				case GameSettings.ControlSchemes.ArrowKeys:
					playerInputs[i] = new Vector2((Input.GetKey(KeyCode.UpArrow) ? 1 : 0) -
												      (Input.GetKey(KeyCode.DownArrow) ? 1 : 0),
												  (Input.GetKey(KeyCode.RightArrow) ? 1 : 0) -
													  (Input.GetKey(KeyCode.LeftArrow) ? 1 : 0));
					break;
				case GameSettings.ControlSchemes.IJKL:
					playerInputs[i] = new Vector2((Input.GetKey(KeyCode.I) ? 1 : 0) -
												      (Input.GetKey(KeyCode.K) ? 1 : 0),
												  (Input.GetKey(KeyCode.L) ? 1 : 0) -
													  (Input.GetKey(KeyCode.J) ? 1 : 0));
					break;
					
				case GameSettings.ControlSchemes.Gamepad1:
					playerInputs[i] = InControl.InputManager.Devices[0].LeftStick;
					break;
				case GameSettings.ControlSchemes.Gamepad2:
					playerInputs[i] = InControl.InputManager.Devices[1].LeftStick;
					break;
				case GameSettings.ControlSchemes.Gamepad3:
					playerInputs[i] = InControl.InputManager.Devices[2].LeftStick;
					break;
				case GameSettings.ControlSchemes.Gamepad4:
					playerInputs[i] = InControl.InputManager.Devices[3].LeftStick;
					break;

				default:
					throw new NotImplementedException(GameSettings.PlayerControlSchemes[i].ToString());
			}
		}
	}
}