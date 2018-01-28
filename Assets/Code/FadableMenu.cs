using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadableMenu : MonoBehaviour {
	private float fadePoint;
	public bool fading;
	public bool visible;

	// Use this for initialization
	void Start () {
		this.fadePoint = 0f;
		this.visible = true;
		this.fading = false;
	}
	
	// Update is called once per frame
	void Update () {
		if(fading && visible){
			this.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(1f, 0f, this.fadePoint);
			this.fadePoint += Time.deltaTime;
			if(this.fadePoint >= 1f){
				this.fading = false;
				this.visible = false;
			}
		}
		if(fading && !visible){
			this.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(0f, 1f, this.fadePoint);
			this.fadePoint += Time.deltaTime;
			if(this.fadePoint >= 1f){
				this.fading = false;
				this.visible = true;
			}
		}
	}

	public void fadeIn(){
		this.fadePoint = 0f;
		this.visible = false;
		this.fading = true;
	}

	public void fadeOut(){
		this.fadePoint = 0f;
		this.visible = true;
		this.fading = true;
	}

	public void show(){
		this.fading = false;
		this.visible = true;
		this.GetComponent<CanvasGroup>().alpha = 1f;
	}

	public void hide(){
		this.fading = false;
		this.visible = false;
		this.GetComponent<CanvasGroup>().alpha = 0f;
	}

}
