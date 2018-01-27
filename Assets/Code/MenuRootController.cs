using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuRootController : MonoBehaviour {
	public GameObject[] Targets;
	public int firstTarget;
	private bool deadlerp;
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
	}

	void FixedUpdate(){
		if(!deadlerp){
			this.transform.position = Vector3.Lerp(this.transform.position, this.currentTarget.transform.position, (Time.deltaTime - this.lerpStart));
			this.transform.rotation = Quaternion.Lerp(this.transform.rotation, this.currentTarget.transform.rotation, (Time.deltaTime - this.lerpStart));
			if(Vector3.Distance(this.transform.position, this.currentTarget.transform.position) < 0.001){
				this.deadlerp = true;
				Debug.Log("Moved close enough to " + this.currentTarget.name + ", stopping.");
			}
		}
	}
	
	public void lerpTo(int targetId){
		this.currentTarget = this.Targets[targetId];
		Debug.Log("Moving to " + this.currentTarget.name);
		this.lerpStart = Time.time;
		this.deadlerp = false;
	}
}
