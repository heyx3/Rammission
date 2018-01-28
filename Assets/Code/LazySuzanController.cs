using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LazySuzanController : MonoBehaviour {
	public float spinSpeed;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.RotateAround(this.transform.position, this.transform.up, Time.deltaTime * this.spinSpeed);
	}
}
