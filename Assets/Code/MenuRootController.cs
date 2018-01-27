using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuRootController : MonoBehaviour {
	public GameObject[] Targets;
	public int firstTarget;
	private bool deadlerp;
	private GameObject currentTarget;
	void Awake(){
	}
	// Use this for initialization
	void Start () {
		this.deadlerp = false;
		this.currentTarget = this.Targets[this.firstTarget];
	}
	
	// Update is called once per frame
	void Update () {
	}

	void FixedUpdate(){
		if(!deadlerp){
			this.transform.position = Vector3.Lerp(this.transform.position, this.currentTarget.transform.position, Time.deltaTime);
			if(Vector3.Distance(this.transform.position, this.currentTarget.transform.position) < 0.001){
				this.deadlerp = true;
				Debug.Log("Moved close enough to " + this.currentTarget.name + ", stopping.");
			}
		}
	}
}
