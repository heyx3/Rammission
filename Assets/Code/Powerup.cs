using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Powerup : MonoBehaviour
{
	public GameObject CreatedEffectPrefab, CollectedEffectPrefab;
	public float TimeTillCollectible = 1.25f;
	public float EffectLength = 5.0f;
	public string Name = "NO NAME";

	public bool IsCollectible { get { return TimeTillCollectible <= 0.0f; } }

	private void Start()
	{
		if (CreatedEffectPrefab != null)
		{
			var effectTr = Instantiate(CreatedEffectPrefab).transform;
			effectTr.SetParent(transform);
			effectTr.localPosition = Vector3.zero;
		}
	}
	private void Update()
	{
		TimeTillCollectible -= Time.deltaTime;
	}
	private void OnDestroy()
	{
		var collectedEffect = Instantiate(CollectedEffectPrefab);
		collectedEffect.transform.position = transform.position;

		
		var text = collectedEffect.GetComponentInChildren<TextMesh>();
		if (text != null)
		{
			text.text = Name;
			text.transform.forward = -(Camera.main.transform.position -
									   text.transform.position).normalized;
		}
	}
}