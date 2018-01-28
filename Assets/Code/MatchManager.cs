using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class MatchManager : MonoBehaviour
{
	public static MatchManager Instance { get; private set; }


	public GameObject PhysObjPrefab;
	public GameObject[] PowerupPrefabs = new GameObject[0];
	public float Radius = 25.0f;
	public float StartY = 1.0f;
	public float MinPowerupSpawnTime = 7.0f,
				 MaxPowerupSpawnTime = 15.0f;

	public IEnumerable<PhysicsObj> PhysicsObjs { get { return objs; } }

	[SerializeField]
	private List<Transform> ringsToDrop = new List<Transform>();
	[SerializeField]
	private List<float> ringDropTimes = new List<float>();
	[SerializeField]
	private AnimationCurve ringDropCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);

	private int[] nPiecesPerPlayer;
	private List<PhysicsObj> objs;


	private void Awake()
	{
		Instance = this;

		objs = new List<PhysicsObj>(GameSettings.NObjectsInField);
		for (int i = 0; i < GameSettings.NObjectsInField; ++i)
		{
			var obj = Instantiate(PhysObjPrefab);
			objs.Add(obj.GetComponent<PhysicsObj>());
			obj.transform.position = RandomPosInArena();
			objs[i].OnConvertedOrKilled += Callback_PieceConvertedOrKilled;
		}
		var neutralObjs = objs.ToList();

		nPiecesPerPlayer = new int[GameSettings.NPlayers];
		for (int playerI = 0; playerI < GameSettings.NPlayers; ++playerI)
		{
			nPiecesPerPlayer[playerI] = 0;
			for (int objI = 0; objI < GameSettings.NObjectsPerPlayer; ++objI)
			{
				nPiecesPerPlayer[playerI] += 1;
				int i = UnityEngine.Random.Range(0, neutralObjs.Count);
				neutralObjs[i].PlayerID = playerI;
				neutralObjs.RemoveAt(i);
			}
		}
		foreach (var obj in neutralObjs)
			obj.PlayerID = -1;

		for (int i = 0; i < ringsToDrop.Count; ++i)
			if (GameSettings.RingOut)
				Destroy(ringsToDrop[i].gameObject);
			else
				StartCoroutine(DropRingCoroutine(ringsToDrop[i], ringDropTimes[i]));

		StartCoroutine(SpawnPowerupCoroutine());
	}

	/// <summary>
	/// Ends the game.
	/// </summary>
	/// <param name="winnerID">
	/// The winner. Pass -1 if nobody wins.
	/// </param>
	public void EndGame(int winnerID)
	{
		Debug.Log(winnerID == -1 ?
				      "No player wins!" :
					  ("Player " + winnerID + " wins!"));
	}

	public Vector3 RandomPosInArena()
	{
		return Quaternion.AngleAxis(UnityEngine.Random.Range(0.0f, 360.0f), Vector3.up) *
			   new Vector3(UnityEngine.Random.Range(0.0f, Radius), StartY, 0.0f);
	}

	private void Callback_PieceConvertedOrKilled(PhysicsObj obj, int oldID, int? newID)
	{
		if (oldID >= 0)
			nPiecesPerPlayer[oldID] -= 1;
		if (newID.HasValue && newID.Value >= 0)
			nPiecesPerPlayer[newID.Value] += 1;

		int nPlayersWithPieces = nPiecesPerPlayer.Count(n => n > 0);
		if (nPlayersWithPieces <= 1)
			EndGame(nPiecesPerPlayer.IndexOf(n => n > 0));
	}

	private System.Collections.IEnumerator DropRingCoroutine(Transform tr, float ringDropTime)
	{
		tr.gameObject.SetActive(true);

		float startY = tr.position.y,
			  endY = 0.0f;
		Vector2 horzPos = tr.position.Horz();

		float t = 0.0f;
		while (t < 1.0f)
		{
			tr.position = new Vector3(horzPos.x,
									  Mathf.Lerp(startY, endY, ringDropCurve.Evaluate(t)),
									  horzPos.y);
			t += Time.deltaTime / ringDropTime;
			yield return null;
		}
		tr.position = new Vector3(horzPos.x, 0.0f, horzPos.y);

		ringsToDrop.Remove(tr);
		if (ringsToDrop.Count == 0)
		{
			Debug.LogError("Start game now!");
		}
	}
	private System.Collections.IEnumerator SpawnPowerupCoroutine()
	{
		while (true)
		{
			yield return new WaitForSeconds(UnityEngine.Random.Range(MinPowerupSpawnTime / GameSettings.PowerupFrequency,
																	 MaxPowerupSpawnTime / GameSettings.PowerupFrequency));

			//Spawn a powerup at a random position.
			var prefab = PowerupPrefabs[UnityEngine.Random.Range(0, PowerupPrefabs.Length)];
			Instantiate(prefab).transform.position = RandomPosInArena();
		}
	}
}