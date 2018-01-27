using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class RotateAroundOrigin : MonoBehaviour
{
	public float TurnSpeed = 1.0f,
				 LookVerticalOffset = -10.0f;

	private Transform tr;

	private void Awake()
	{
		tr = transform;
	}
	private void Update()
	{
		tr.position = Quaternion.AngleAxis(TurnSpeed * Time.deltaTime, Vector3.up) *
					  tr.position;
		tr.forward = (-tr.position + new Vector3(0.0f, LookVerticalOffset, 0.0f)).normalized;
	}
}