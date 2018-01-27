using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Spawner : MonoBehaviour
{
	public int NToSpawn = 30,
			   NPlayers = 2,
			   NPerPlayer = 5;
	public GameObject Prefab;
	public float Bounds = 50.0f;
	public float StartY = 1.0f;


	private void Awake()
	{
		var objs = new List<GameObject>(NToSpawn);
		for (int i = 0; i < NToSpawn; ++i)
		{
			var obj = Instantiate(Prefab);
			objs.Add(obj);
			obj.transform.position = new Vector3(UnityEngine.Random.Range(-Bounds * 0.5f, Bounds * 0.5f),
												 StartY,
												 UnityEngine.Random.Range(-Bounds * 0.5f, Bounds * 0.5f));
		}

		for (int playerI = 0; playerI < NPlayers; ++playerI)
		{
			for (int objI = 0; objI < NPerPlayer; ++objI)
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