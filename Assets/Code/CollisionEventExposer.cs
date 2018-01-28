using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Provides events for when this object collides.
/// </summary>
public class CollisionEventExposer : MonoBehaviour
{
	public event Action<GameObject, Collision> CollisionEnter,
											   CollisionStay,
											   CollisionExit;
	
	public event Action<GameObject, Collider> TriggerEnter,
											  TriggerStay,
											  TriggerExit;


	private void OnCollisionEnter(Collision collision)
	{
		if (CollisionEnter != null)
			CollisionEnter(gameObject, collision);
	}
	private void OnCollisionStay(Collision collision)
	{
		if (CollisionStay != null)
			CollisionStay(gameObject, collision);
	}
	private void OnCollisionExit(Collision collision)
	{
		if (CollisionExit != null)
			CollisionExit(gameObject, collision);
	}

	private void OnTriggerEnter(Collider collision)
	{
		if (TriggerEnter != null)
			TriggerEnter(gameObject, collision);
	}
	private void OnTriggerStay(Collider collision)
	{
		if (TriggerStay != null)
			TriggerStay(gameObject, collision);
	}
	private void OnTriggerExit(Collider collision)
	{
		if (TriggerExit != null)
			TriggerExit(gameObject, collision);
	}
}