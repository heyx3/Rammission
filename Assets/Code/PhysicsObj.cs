using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Rigidbody))]
public class PhysicsObj : MonoBehaviour
{
	public int PlayerID = -1;
	public float Acceleration = 10.0f,
				 TurnSpeed = 6.0f;

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

	private void Awake()
	{
		rgd = GetComponent<Rigidbody>();
		rnd = GetComponent<Renderer>();
	}
	private void FixedUpdate()
	{
		rnd.material = PlayerMaterials[PlayerID + 1];

		if (PlayerID < 0)
			return;


		float direction = 0.0f;
		if (Input.GetKey(Control_Forward[PlayerID]))
			direction += 1.0f;
		if (Input.GetKey(Control_Backward[PlayerID]))
			direction -= 1.0f;

		//rgd.AddForce(transform.forward * direction * Acceleration * Time.deltaTime,
		//			 ForceMode.Impulse);
		rgd.velocity += transform.forward * direction * Acceleration * Time.deltaTime;
		
		float turnDir = 0.0f;
		if (Input.GetKey(Control_TurnLeft[PlayerID]))
			turnDir -= 1.0f;
		if (Input.GetKey(Control_TurnRight[PlayerID]))
			turnDir += 1.0f;

		transform.Rotate(new Vector3(0.0f, turnDir * TurnSpeed * Time.deltaTime, 0.0f),
						 Space.World);
		var rot = transform.eulerAngles;
		transform.eulerAngles = new Vector3(0.0f, rot.y, 0.0f);
	}
}