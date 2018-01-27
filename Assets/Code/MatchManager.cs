using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MatchManager : MonoBehaviour
{
	public GameObject Prefab;
	public float Radius = 25.0f;
	public float StartY = 1.0f;

	[SerializeField]
	private List<Transform> ringsToDrop = new List<Transform>();
	[SerializeField]
	private List<float> ringDropTimes = new List<float>();
	[SerializeField]
	private AnimationCurve ringDropCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);

	private int[] nPiecesPerPlayer;
	private List<PhysicsObj> objs;

	private void Awake(){
		// hold off on starting the match we need to find out what game type.
		//SceneManager.LoadScene("Scenes/MenuScene", LoadSceneMode.Additive);		


		this.MatchStart();
	}

	private void MatchStart()
	{
		objs = new List<PhysicsObj>(GameSettings.NObjectsInField);
		for (int i = 0; i < GameSettings.NObjectsInField; ++i)
		{
			var obj = Instantiate(Prefab);
			objs.Add(obj.GetComponent<PhysicsObj>());
			obj.transform.position = Quaternion.AngleAxis(UnityEngine.Random.Range(0.0f, 360.0f), Vector3.up) *
									 new Vector3(UnityEngine.Random.Range(0.0f, Radius), StartY, 0.0f);
			objs[i].OnConvertedOrKilled += Callback_PieceConvertedOrKilled;
		}

		nPiecesPerPlayer = new int[GameSettings.NPlayers];
		for (int playerI = 0; playerI < GameSettings.NPlayers; ++playerI)
		{
			nPiecesPerPlayer[playerI] = 0;
			for (int objI = 0; objI < GameSettings.NObjectsPerPlayer; ++objI)
			{
				nPiecesPerPlayer[playerI] += 1;
				int i = UnityEngine.Random.Range(0, objs.Count);
				objs[i].PlayerID = playerI;
				objs.RemoveAt(i);
			}
		}

		foreach (var obj in objs)
			obj.PlayerID = -1;

		for (int i = 0; i < ringsToDrop.Count; ++i)
			if (GameSettings.RingOut)
				Destroy(ringsToDrop[i].gameObject);
			else
				StartCoroutine(DropRingCoroutine(ringsToDrop[i], ringDropTimes[i]));
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
}