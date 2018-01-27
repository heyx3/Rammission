using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Spawner : MonoBehaviour
{
	public GameObject Prefab;
	public float Bounds = 50.0f;
	public float StartY = 1.0f;


	private void Awake()
	{
		var objs = new List<GameObject>(GameSettings.NObjectsInField);
		for (int i = 0; i < GameSettings.NObjectsInField; ++i)
		{
			var obj = Instantiate(Prefab);
			objs.Add(obj);
			obj.transform.position = new Vector3(UnityEngine.Random.Range(-Bounds * 0.5f, Bounds * 0.5f),
												 StartY,
												 UnityEngine.Random.Range(-Bounds * 0.5f, Bounds * 0.5f));
		}

		for (int playerI = 0; playerI < GameSettings.NPlayers; ++playerI)
		{
			for (int objI = 0; objI < GameSettings.NObjectsPerPlayer; ++objI)
			{
				int i = UnityEngine.Random.Range(0, objs.Count);
				objs[i].GetComponent<PhysicsObj>().PlayerID = playerI;
				objs.RemoveAt(i);
			}
		}

		foreach (var obj in objs)
			obj.GetComponent<PhysicsObj>().PlayerID = -1;

		Destroy(this);
	}
}