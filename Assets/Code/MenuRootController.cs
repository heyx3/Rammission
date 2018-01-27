using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuRootController : MonoBehaviour {
	public GameObject[] Targets;
	public int firstTarget;
	private bool deadlerp = true;
	private GameObject currentTarget;
	private float lerpStart;
	void Awake(){
	}
	// Use this for initialization
	void Start () {
		this.lerpTo(this.firstTarget);
	}
	
	// Update is called once per frame
	void Update () {
		// I dunno, check something
		
	}

	void FixedUpdate(){
		if(!deadlerp){
			this.transform.position = Vector3.Lerp(this.transform.position, this.currentTarget.transform.position, Time.deltaTime);
			this.transform.rotation = Quaternion.Lerp(this.transform.rotation, this.currentTarget.transform.rotation, Time.deltaTime);
			if(Vector3.Distance(this.transform.position, this.currentTarget.transform.position) < 0.001){
				this.deadlerp = true;
				Debug.Log("Moved close enough to " + this.currentTarget.name + ", stopping.");
			}
		}
	}
	
	public void lerpTo(int targetId){
		Debug.Log("Hello, let's LERP! Last time we did this it was " + this.lerpStart);
		this.currentTarget = this.Targets[targetId];
		Debug.Log("Moving to " + this.currentTarget.name);
		this.lerpStart = Time.time;
		Debug.Log("Lerping from " + this.lerpStart);
		this.deadlerp = false;
	}
}
