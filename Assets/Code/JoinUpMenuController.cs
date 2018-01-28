using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class JoinUpMenuController : MonoBehaviour
{
	[Serializable]
	public struct SchemeAndObject
	{
		public GameSettings.ControlSchemes Scheme;
		public GameObject Object;
		public SchemeAndObject(GameSettings.ControlSchemes scheme, GameObject obj)
		{
			Scheme = scheme;
			Object = obj;
		}
	}

	public SchemeAndObject[] SchemeObjs = new SchemeAndObject[]
	{
		new SchemeAndObject(GameSettings.ControlSchemes.WASD, null),
		new SchemeAndObject(GameSettings.ControlSchemes.ArrowKeys, null),
		new SchemeAndObject(GameSettings.ControlSchemes.IJKL, null),
		new SchemeAndObject(GameSettings.ControlSchemes.Gamepad1, null),
		new SchemeAndObject(GameSettings.ControlSchemes.Gamepad2, null),
		new SchemeAndObject(GameSettings.ControlSchemes.Gamepad3, null),
		new SchemeAndObject(GameSettings.ControlSchemes.Gamepad4, null),
	};
	public GameObject NextScreenButton;

	private bool update = false;


	private void Awake()
	{
		foreach (var obj in SchemeObjs.Select(sch => sch.Object))
			obj.SetActive(false);
		NextScreenButton.SetActive(false);

		update = false;
	}
	public void AwakeMe()
	{
		foreach (var obj in SchemeObjs.Select(sch => sch.Object))
			obj.SetActive(false);
		NextScreenButton.SetActive(false);
		
		GameSettings.PlayerControlSchemes.Clear();
		GameSettings.NPlayers = 0;

		update = true;
	}
	private void Start()
	{
		NextScreenButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(
			() =>
			{
				foreach (var obj in SchemeObjs.Select(sch => sch.Object))
					obj.SetActive(false);
				NextScreenButton.SetActive(false);

				update = false;

				MatchManager.Instance.MatchStart();
				UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("Scenes/MenuScene");
			}));
	}
	private void Update()
	{
		if (!update)
			return;

		UpdateKeys(KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D, GameSettings.ControlSchemes.WASD);
		UpdateKeys(KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow,
				   GameSettings.ControlSchemes.ArrowKeys);
		UpdateKeys(KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L, GameSettings.ControlSchemes.IJKL);
		UpdateGamepad(0, GameSettings.ControlSchemes.Gamepad1);
		UpdateGamepad(1, GameSettings.ControlSchemes.Gamepad2);
		UpdateGamepad(2, GameSettings.ControlSchemes.Gamepad3);
		UpdateGamepad(3, GameSettings.ControlSchemes.Gamepad4);
	}

	private void UpdateKeys(KeyCode k1, KeyCode k2, KeyCode k3, KeyCode k4,
							GameSettings.ControlSchemes scheme)
	{
		if (Input.GetKeyDown(k1) || Input.GetKeyDown(k2) || Input.GetKeyDown(k3) || Input.GetKeyDown(k4))
			EnableScheme(scheme);
	}
	private void UpdateGamepad(int i, GameSettings.ControlSchemes scheme)
	{
		if (InControl.InputManager.Devices.Count > i &&
			InControl.InputManager.Devices[i].LeftStick.Vector.magnitude > 0.1f)
		{
			EnableScheme(scheme);
		}
	}
	private void EnableScheme(GameSettings.ControlSchemes scheme)
	{
		int i = SchemeObjs.IndexOf(sch => sch.Scheme == scheme);
		SchemeObjs[i].Object.SetActive(true);

		GameSettings.PlayerControlSchemes.Add(scheme);
		GameSettings.NPlayers = GameSettings.PlayerControlSchemes.Count;

		NextScreenButton.SetActive(GameSettings.NPlayers > 1);
	}
}