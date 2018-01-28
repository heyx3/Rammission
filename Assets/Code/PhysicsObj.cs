using System;
using System.Collections;
using System.Collections.Generic;
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

	public GameObject Prefab;
	public Material NormalEyes, AngryEyes, ScaredEyes;
	public Renderer EyesRenderer;
	public GameObject HitEffects;


	public List<KeyCode> Control_Power_One = new List<KeyCode> ()
	{
		KeyCode.E,
		KeyCode.L
	};
	public List<KeyCode> Control_Power_Two = new List<KeyCode> ()
	{
		KeyCode.Q,
		KeyCode.K
	};

	public List<Material> PlayerMaterials = new List<Material>();


	private Rigidbody rgd;
	private Renderer rnd;
	private HashSet<PhysicsObj> currentCollidingObjs = new HashSet<PhysicsObj>();
	private HashSet<Transform> riverFlows = new HashSet<Transform>();
	private float timeWithCollisions = 0.0001f;

	private float powerUpTime = 5.0f;
	private bool hasPowered;
	private float scaleMultiply = 1.5f;


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
	private void FixedUpdate()
	{
		if (rnd != null)
			rnd.material = PlayerMaterials[PlayerID + 1];

		if (rgd == null)
			return;


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


	private void PowerUpdate ()
	{
		StartCoroutine (PowerOneUpdate ());
		//PowerOneUpdate ();
		StartCoroutine (PowerTwoUpdate ());
	}


	IEnumerator PowerOneUpdate()
	{	
		if (Input.GetKeyUp (Control_Power_One [PlayerID]) && !hasPowered) 
		{
			Split ();
			yield return new WaitForSeconds (powerUpTime);
			Destroy (this.gameObject);
			hasPowered = false;
		}
			
	}


	IEnumerator PowerTwoUpdate()
	{
		if (Input.GetKeyDown (Control_Power_Two [PlayerID]) && !hasPowered) 
		{
			Enlarge ();
			yield return new WaitForSeconds (powerUpTime);
			Shrink ();
			hasPowered = false;
		}
			
	}

	private void Update ()
	{
		if (PlayerID < 0)
			return;
		PowerUpdate ();
	}

	private void Split()
	{
		GameObject obj = Instantiate (Prefab);
		var physObj = obj.GetComponent<PhysicsObj> ();
		physObj.PlayerID = PlayerID;
	
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

		hasPowered = true;
	}

	private void SpeedUp()
	{
		rgd.velocity *= 2.0f;
		hasPowered = true;
	}

	private void SpeedDown()
	{
		rgd.velocity /= 2.0f;
		hasPowered = true;
	}

	private void Shrink () 
	{
		transform.localScale /= scaleMultiply;
		hasPowered = true;
	}

	private void Enlarge()
	{
		transform.localScale *= scaleMultiply;
		hasPowered = true;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "River Flow Trigger")
			riverFlows.Add(other.transform);
	}
	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.tag == "River Flow Trigger")
			riverFlows.Remove(other.transform);
	}
}