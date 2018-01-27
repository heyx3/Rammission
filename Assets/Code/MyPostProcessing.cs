using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PostProcessing;


[RequireComponent(typeof(PostProcessingBehaviour))]
public class MyPostProcessing : MonoBehaviour
{
	private Transform tr;
	private PostProcessingProfile profile;


	private void Start()
	{
		tr = transform;
		profile = GetComponent<PostProcessingBehaviour>().profile;
	}
	private void Update()
	{
		var dofSetting = profile.depthOfField.settings;
		dofSetting.focusDistance = tr.position.magnitude;
		profile.depthOfField.settings = dofSetting;
	}
}