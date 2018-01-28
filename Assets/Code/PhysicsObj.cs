using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class PhysicsObj : MonoBehaviour
{
	/// <summary>
	/// Raised when this object is converted or destroyed.
	/// The first int is the old player ID (if it was converted).
	/// The second int is its new/current player ID (or null if it was killed).
	/// </summary>
	public event Action<PhysicsObj, int, int?> OnConvertedOrKilled;

	public int PlayerID = -1;
	public float Acceleration = 10.0f,
				 TurnSpeed = 6.0f;
	public float AngryEyesThreshold = 0.0f;
	public float KillHeight = -10.0f;
	public float PushAwayStrength = 10.0f;
	public float RiverFlowStrength = 10.0f;
	
	public float PowerUpSpeedScale = 2.0f;
	public float PowerUpScale = 1.5f;
	
	public Material NormalEyes, AngryEyes, ScaredEyes;
	public Renderer EyesRenderer;
	public GameObject HitEffects;

	public List<Material> PlayerMaterials = new List<Material>();

	[SerializeField]
	private bool startsInMap = false;


	private Rigidbody rgd;
	private Renderer rnd;
	private HashSet<PhysicsObj> currentCollidingObjs = new HashSet<PhysicsObj>();
	private HashSet<Transform> riverFlows = new HashSet<Transform>();
	private float timeWithCollisions = 0.0001f;

	private bool isPowered = false;


	private void Awake()
	{
		rgd = GetComponentInChildren<Rigidbody>();
		rnd = GetComponentInChildren<Renderer>();

		var collisionEvents = GetComponentInChildren<CollisionEventExposer>();
		collisionEvents.CollisionEnter += (obj, coll) => OnCollisionEnter(coll);
		collisionEvents.CollisionExit += (obj, coll) => OnCollisionExit(coll);
		collisionEvents.TriggerEnter += (obj, coll) => OnTriggerEnter(coll);
		collisionEvents.TriggerExit += (obj, coll) => OnTriggerExit(coll);
	}
	private void Start()
	{
		if (startsInMap)
			MatchManager.Instance.HeyTheresANewPhysObj(this);
	}
	private void FixedUpdate()
	{
		if (rnd != null)
			rnd.material = PlayerMaterials[PlayerID + 1];


		//Get acceleration from rivers.
		Vector2 accel = Vector2.zero;
		foreach (var riverFlow in riverFlows)
			accel += riverFlow.forward.Horz().normalized;
		accel = accel.normalized * RiverFlowStrength;

		if (PlayerID >= 0)
		{
			var moveInput = MyInput.GetInput(PlayerID);

			accel += transform.forward.Horz().normalized * moveInput.y * Acceleration;
			transform.Rotate(new Vector3(0.0f, moveInput.x * TurnSpeed * Time.deltaTime, 0.0f),
							 Space.World);
		}

		//Accelerate forwards/backwards.
		rgd.velocity += accel.To3D() * Time.deltaTime;
	}
	private void LateUpdate()
	{
		if (transform.position.y <= KillHeight)
		{
			if (OnConvertedOrKilled != null)
				OnConvertedOrKilled(this, PlayerID, null);

			Destroy(gameObject);
			return;
		}

		if (rgd != null)
		{
			transform.position = rgd.position;
			rgd.transform.localPosition = Vector3.zero;
		}

		//If this object is controlled by a player, give it some eyes.
		if (PlayerID >= 0)
		{
			EyesRenderer.enabled = true;

			//Update the timing until the object displays angry eyes.
			if (currentCollidingObjs.Count > 0)
				timeWithCollisions += Time.deltaTime;
			else
				timeWithCollisions = 0.0f;

			if (timeWithCollisions >= AngryEyesThreshold)
				EyesRenderer.sharedMaterial = AngryEyes;
			else
				EyesRenderer.sharedMaterial = NormalEyes;
		}
		else
			EyesRenderer.enabled = false;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.transform.parent == null)
			return;

		var otherObj = collision.transform.parent.GetComponent<PhysicsObj>();
		if (otherObj == null)
			return;

		currentCollidingObjs.Add(otherObj);

		//To eliminate duplicate computations,
		//    have the object with the lowest ID run this code.
		//Note that objects with the same ID will never run this code,
		//    because we don't care about them colliding.
		if (PlayerID >= otherObj.PlayerID)
			return;

		Transform tr = transform,
				  otherTr = otherObj.transform;

		Vector2 thisPos = tr.position.Horz(),
				otherPos = otherTr.position.Horz();
		Vector2 toOther = (otherPos - thisPos).normalized;

		float thisDot = Vector2.Dot(rgd.velocity.Horz(), toOther),
			  otherDot = Vector2.Dot(otherObj.rgd.velocity.Horz(), -toOther);
		thisDot *= rgd.mass;
		otherDot *= otherObj.rgd.mass;
		
		//If there is a tie, randomly decide the winner.
		thisDot += UnityEngine.Random.Range(-0.0001f, 0.0001f);

		//Only count it as a capture if
		//    at least one of them is actually facing towards the other.
		if (thisDot > 0.0f | otherDot > 0.0f)
		{
			Instantiate(HitEffects).transform.position =
				collision.contacts[0].point;

			if (thisDot < otherDot)
			{
				if (otherObj.PlayerID >= 0)
					Captured(otherObj);
			}
			else
			{
				if (PlayerID >= 0)
					otherObj.Captured(this);
			}
		}
	}
	private void Captured(PhysicsObj capturer)
	{
		//There are different rules when capturing a neutral piece vs an enemy piece.

		Vector2 towardsCapturer = (capturer.transform.position.Horz() -
								   transform.position.Horz()).normalized;
		rgd.velocity += (towardsCapturer * -PushAwayStrength).To3D();
		capturer.rgd.velocity += (towardsCapturer * PushAwayStrength).To3D();

		if (PlayerID == -1)
		{
			SetNewID(capturer.PlayerID);

			if (GameSettings.TransferWhenCaptureNeutral)
			{
				transform.forward = capturer.transform.forward;
				capturer.SetNewID(-1);
			}
		}
		else switch (GameSettings.EnemyCaptureMode)
		{
			case GameSettings.EnemyCaptureModes.Convert:
				SetNewID(capturer.PlayerID);
			break;

			case GameSettings.EnemyCaptureModes.Destroy:
				SetNewID(null);
				break;

			case GameSettings.EnemyCaptureModes.Transfer:
				transform.forward = capturer.transform.forward;
				SetNewID(capturer.PlayerID);
				capturer.SetNewID(-1);
				break;
		}
	}
	/// <summary>
	/// Sets this object's ID (or destroys it if "null" is passed).
	/// </summary>
	private void SetNewID(int? newID)
	{
		int oldID = PlayerID;
		if (newID.HasValue)
			PlayerID = newID.Value;

		if (OnConvertedOrKilled != null)
			OnConvertedOrKilled(this, oldID, newID);

		if (!newID.HasValue)
			Destroy(gameObject);
	}

	private void OnCollisionExit(Collision collision)
	{
		if (collision.transform.parent == null)
			return;

		var obj = collision.transform.parent.GetComponent<PhysicsObj>();
		if (obj == null)
			return;

		currentCollidingObjs.Remove(obj);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "River Flow Trigger")
		{
			riverFlows.Add(other.transform);
		}
		else if (PlayerID >= 0 && !isPowered)
		{
			var powerup = other.GetComponent<Powerup>();
			if (powerup != null && powerup.IsCollectible)
			{
				switch (other.gameObject.tag)
				{
					case "Split Powerup":
						DoToAllTeammates(obj => obj.Split(powerup.EffectLength));
						break;
					case "Speedup Powerup":
						DoToAllTeammates(obj => obj.SpeedUp(powerup.EffectLength));
						break;
					case "Speeddown Powerup":
						DoToAllTeammates(obj => obj.SpeedDown(powerup.EffectLength));
						break;
					case "Shrink Powerup":
						DoToAllTeammates(obj => obj.Shrink(powerup.EffectLength));
						break;
					case "Enlarge Powerup":
						DoToAllTeammates(obj => obj.Enlarge(powerup.EffectLength));
						break;
					case "Realign Powerup":
						Realign();
						break;

					default:
						Debug.LogWarning("Collision with unknown powerup " + other.gameObject.name);
						break;
				}
				
				powerup.Collect();
			}
		}
	}
	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.tag == "River Flow Trigger")
			riverFlows.Remove(other.transform);
	}
	
	IEnumerator Timer(float time, Action toDo)
	{
		yield return new WaitForSeconds(time);
		toDo();
	}
	
	private void DoToAllTeammates(Action<PhysicsObj> toDo)
	{
		var allies = MatchManager.Instance.PhysicsObjs.Where (obj => obj.PlayerID == PlayerID).ToList ();
		foreach (var ally in allies)
			toDo(ally);
	}
	private void Split(float time)
	{
		GameObject obj = Instantiate (gameObject);
		var physObj = obj.GetComponent<PhysicsObj> ();
		physObj.PlayerID = PlayerID;
		MatchManager.Instance.HeyTheresANewPhysObj (physObj);
	
		float angleIncre = Mathf.PI / 4;
	
		Vector3 divertVec = new Vector3 (rgd.velocity.x*(1+Mathf.Cos(angleIncre)), 0.0f, rgd.velocity.z);
		physObj.rgd.velocity = rgd.velocity+divertVec;
	
		float objScale = obj.transform.localScale.x;
		float offset = Mathf.Clamp (
			Mathf.Lerp(objScale, 1.5f*objScale, UnityEngine.Random.value),
			objScale,
			1.5f*objScale
			);
		Vector3 posDiffVec = new Vector3(offset, 0, offset);
		obj.transform.position = transform.position + posDiffVec;

		isPowered = true;
		
		StartCoroutine (Timer (time, () =>
		{
			if (OnConvertedOrKilled != null)
				OnConvertedOrKilled(this, PlayerID, null);
			Destroy (gameObject);
		}));
	}
	private void SpeedUp(float time)
	{
		rgd.velocity *= PowerUpSpeedScale;
		isPowered = true;
		StartCoroutine(Timer(time, () => { isPowered = false; if (rgd != null) rgd.velocity /= PowerUpSpeedScale; }));
	}
	private void SpeedDown(float time)
	{
		rgd.velocity /= PowerUpSpeedScale;
		isPowered = true;
		StartCoroutine(Timer(time, () => { isPowered = false; if (rgd != null) rgd.velocity *= PowerUpSpeedScale; }));
	}
	private void Shrink(float time) 
	{
		transform.localScale /= PowerUpScale;
		rgd.mass /= PowerUpScale;
		isPowered = true;
		StartCoroutine(Timer(time, () => { isPowered = false; if (rgd != null) rgd.mass *= PowerUpScale; transform.localScale *= PowerUpScale; }));
	}
	private void Enlarge(float time)
	{
		transform.localScale *= PowerUpScale;
		rgd.mass *= PowerUpScale;
		isPowered = true;
		StartCoroutine(Timer(time, () => { isPowered = false; if (rgd != null) rgd.mass /= PowerUpScale; transform.localScale /= PowerUpScale; }));
	}
	private void Realign()
	{
		Vector3 forward = transform.forward;
		foreach (var ally in MatchManager.Instance.PhysicsObjs.Where(obj => obj.PlayerID == PlayerID))
		{
			ally.transform.forward = forward;
			ally.rgd.velocity = rgd.velocity;
		}
	}
}