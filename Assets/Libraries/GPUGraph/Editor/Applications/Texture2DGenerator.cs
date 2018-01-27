using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using GPUGraph;


namespace GPUGraph.Applications
{
	[Serializable]
	public class Texture2DGenerator : TextureGenerator
	{
		[MenuItem("Assets/GPU Graph/Generate 2D Texture", false, 4)]
		public static void ShowWindow()
		{
			ScriptableObject.CreateInstance<Texture2DGenerator>().Show();
		}


		public int X = 128,
				   Y = 128;
		public bool GenerateNormals;
		public float NormalStrength = 1.0f;


		protected override void OnEnable()
		{
			base.OnEnable();

			titleContent = new GUIContent("2D Tex");
			minSize = new Vector2(200.0f, 300.0f + Y);

			if (HasGraph)
				GetPreview(true);
		}

		protected override void OnGUI_BelowGraphSelection()
		{
			EditorGUI.BeginChangeCheck();
			{
				X = EditorGUILayout.DelayedIntField("Width", X);
				Y = EditorGUILayout.DelayedIntField("Height", Y);
			}
			if (EditorGUI.EndChangeCheck())
				GetPreview(false);
			
			GenerateNormals = EditorGUILayout.Toggle("Normal-map", GenerateNormals);
			ShowGradient = !GenerateNormals;
			if (GenerateNormals)
			{
				NormalStrength = EditorGUILayout.DelayedFloatField("Strength", NormalStrength);
			}

			GUILayout.Space(15.0f);
		}

		protected override void GeneratePreview(ref Texture2D outTex, Material noiseMat)
		{
			if (outTex == null || outTex.width != X || outTex.height != Y)
			{
				outTex = new Texture2D(X, Y, TextureFormat.ARGB32, false);
				outTex.wrapMode = TextureWrapMode.Clamp;
				outTex.filterMode = FilterMode.Point;
			}

			GraphUtils.GenerateToTexture(RenderTexture.GetTemporary(outTex.width, outTex.height),
										 noiseMat, outTex, true);
		}
		protected override void GenerateTexture()
		{
			string savePath = EditorUtility.SaveFilePanel("Choose where to save the texture.",
														  Application.dataPath, "MyTex.png", "png");
			if (savePath.Length == 0)
				return;

			//Write out the texture as a PNG.
			Texture2D noiseTex = GetPreview(false);
			if (GenerateNormals)
				ConvertToNormalMap(noiseTex);
			try
			{
				File.WriteAllBytes(savePath, noiseTex.EncodeToPNG());
			}
			catch (Exception e)
			{
				Debug.LogError("Unable to save texture to file: " + e.Message);
			}

			//Finally, open explorer to show the user the texture.
			EditorUtility.RevealInFinder(StringUtils.FixDirectorySeparators(savePath));
		}
		private void ConvertToNormalMap(Texture2D tex)
		{
			//Get the texture values as a bumpmap.
			var pixelColors = tex.GetPixels();
			var bumpmap = new float[tex.width, tex.height];
			for (int y = 0; y < tex.height; ++y)
				for (int x = 0; x < tex.width; ++x)
					bumpmap[x, y] = pixelColors[x + (y * tex.width)].r;

			//Convert to a normal map and pack it into the texture's colors.
			for (int y = 0; y < tex.height; ++y)
			{
				for (int x = 0; x < tex.width; ++x)
				{
					int lessX = (x == 0 ? tex.width - 1 : x - 1),
						moreX = (x == tex.width - 1 ? 0 : x + 1),
						lessY = (y == 0 ? tex.height - 1 : y - 1),
						moreY = (y == tex.height - 1 ? 0 : y + 1);
					Vector2 heightChangeAlongAxes = new Vector2(bumpmap[moreX, y] - bumpmap[lessX, y],
																bumpmap[x, moreY] - bumpmap[x, lessY]);
					var normal = new Vector3(heightChangeAlongAxes.x,
											 NormalStrength,
											 heightChangeAlongAxes.y).normalized;
					pixelColors[x + (y * tex.width)] = new Color(0.5f + (0.5f * normal.x),
																 0.5f + (0.5f * normal.z),
																 0.5f + (0.5f * normal.y),
																 1.0f);
				}
			}

			tex.SetPixels(pixelColors);
		}
	}
}