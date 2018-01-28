using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LazySuzanController : MonoBehaviour
{
	public float spinSpeed;

	private void Start()
	{
		foreach (FadableMenu c in GetComponentsInChildren<FadableMenu>())
		{
			c.hide();
		}
	}
	private void Update()
	{
		transform.RotateAround(transform.position, transform.up, Time.deltaTime * spinSpeed);
	}
	
	public void SetEnemyMode_Convert()
	{
		GameSettings.EnemyCaptureMode = GameSettings.EnemyCaptureModes.Convert;
	}
	public void SetEnemyMode_Transfer()
	{
		GameSettings.EnemyCaptureMode = GameSettings.EnemyCaptureModes.Transfer;
	}
	public void SetEnemyMode_Destroy()
	{
		GameSettings.EnemyCaptureMode = GameSettings.EnemyCaptureModes.Destroy;
	}
	public void SetRingOut(bool ringOut)
	{
		GameSettings.RingOut = ringOut;
	}
}