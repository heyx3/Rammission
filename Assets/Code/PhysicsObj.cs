using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PhysicsObj : MonoBehaviour
{
	public int PlayerID = -1;
	public float Acceleration = 10.0f,
				 TurnSpeed = 6.0f;
	public float AngryEyesThreshold = 0.0f;
	public float KillHeight = -10.0f;

	public Material NormalEyes, AngryEyes, ScaredEyes;
	public Renderer EyesRenderer;

	public List<KeyCode> Control_Forward = new List<KeyCode>()
	{
		KeyCode.W,
		KeyCode.UpArrow
	};
	public List<KeyCode> Control_Backward = new List<KeyCode>()
	{
		KeyCode.S,
		KeyCode.DownArrow
	};
	public List<KeyCode> Control_TurnLeft = new List<KeyCode>()
	{
		KeyCode.A,
		KeyCode.LeftArrow
	};
	public List<KeyCode> Control_TurnRight = new List<KeyCode>()
	{
		KeyCode.D,
		KeyCode.RightArrow
	};
	public List<Material> PlayerMaterials = new List<Material>();


	private Rigidbody rgd;
	private Renderer rnd;
	private HashSet<PhysicsObj> currentCollidingObjs = new HashSet<PhysicsObj>();
	private float timeWithCollisions = 0.0001f;


	private void Awake()
	{
		rgd = GetComponentInChildren<Rigidbody>();
		rnd = GetComponentInChildren<Renderer>();

		var collisionEvents = GetComponentInChildren<CollisionEventExposer>();
		collisionEvents.CollisionEnter += (obj, coll) => OnCollisionEnter(coll);
		collisionEvents.CollisionExit += (obj, coll) => OnCollisionExit(coll);
	}
	private void FixedUpdate()
	{
		if (rnd != null)
			rnd.material = PlayerMaterials[PlayerID + 1];

		if (rgd == null || PlayerID < 0)
			return;

		//Accelerate forwards/backwards.
		float direction = 0.0f;
		if (Input.GetKey(Control_Forward[PlayerID]))
			direction += 1.0f;
		if (Input.GetKey(Control_Backward[PlayerID]))
			direction -= 1.0f;
		rgd.velocity += transform.forward * direction * Acceleration * Time.deltaTime;

		//Turn.
		float turnDir = 0.0f;
		if (Input.GetKey(Control_TurnLeft[PlayerID]))
			turnDir -= 1.0f;
		if (Input.GetKey(Control_TurnRight[PlayerID]))
			turnDir += 1.0f;
		transform.Rotate(new Vector3(0.0f, turnDir * TurnSpeed * Time.deltaTime, 0.0f),
						 Space.World);
	}
	private void LateUpdate()
	{
		if (transform.position.y <= KillHeight)
		{
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

		float thisDot = Vector2.Dot(tr.forward.Horz().normalized, toOther),
			  otherDot = Vector2.Dot(otherTr.forward.Horz().normalized, -toOther);

		//Scale the values by velocity, so that faster players have an advantage.
		thisDot *= rgd.velocity.Horz().magnitude;
		otherDot *= otherObj.rgd.velocity.Horz().magnitude;

		//If there is a tie, randomly decide the winner.
		thisDot += UnityEngine.Random.Range(-0.0001f, 0.0001f);

		//Only count it as a capture if
		//    at least one of them is actually facing towards the other.
		if (thisDot > 0.0f | otherDot > 0.0f)
			if (thisDot < otherDot)
				Captured(otherObj);
			else
				otherObj.Captured(this);
	}
	private void Captured(PhysicsObj capturer)
	{
		if (capturer.PlayerID >= 0)
			PlayerID = capturer.PlayerID;
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
}