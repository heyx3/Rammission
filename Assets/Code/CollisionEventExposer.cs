using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Provides events for when this object collides.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CollisionEventExposer : MonoBehaviour
{
	public event Action<GameObject, Collision> CollisionEnter,
											   CollisionStay,
											   CollisionExit;

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
}