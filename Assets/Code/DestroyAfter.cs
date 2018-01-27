using System;
using UnityEngine;


public class DestroyAfter :MonoBehaviour
{
	public float Time;

	private System.Collections.IEnumerator Start()
	{
		yield return new WaitForSeconds(Time);
		Destroy(gameObject);
	}
}