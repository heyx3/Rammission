using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;


public static class TerrainHeightmapGen
{
	[MenuItem("Terrain Gen/From PNG")]
	public static void GenFromPNG()
	{
		if (Selection.activeObject == null || !(Selection.activeObject is GameObject))
			return;

		var terrain = ((GameObject)Selection.activeObject).GetComponent<Terrain>();
		if (terrain == null)
			return;

		string filePath = EditorUtility.OpenFilePanel("Choose the image", Application.dataPath, "png");
		if (filePath == "")
			return;

		Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
		tex.LoadImage(System.IO.File.ReadAllBytes(filePath), false);

		Undo.RecordObject(terrain, "Set heightmap from PNG");

		var data = terrain.terrainData;
		var heights = new float[tex.width, tex.height];
		for (int y = 0; y < tex.height; ++y)
			for (int x = 0; x < tex.width; ++x)
				heights[x, y] = tex.GetPixel(x, y).r;

		data.SetHeights(0, 0, heights);
	}
	
	[MenuItem("Terrain Gen/Noise")]
	public static void Noise()
	{
		if (Selection.activeObject == null || !(Selection.activeObject is GameObject))
			return;

		var terrain = ((GameObject)Selection.activeObject).GetComponent<Terrain>();
		if (terrain == null)
			return;
		
		Undo.RecordObject(terrain, "Smooth Terrain");

		var data = terrain.terrainData;
		var heights = new float[data.heightmapWidth, data.heightmapHeight];
		for (int x = 0; x < heights.GetLength(0); ++x)
		{
			for (int y = 0; y < heights.GetLength(1); ++y)
			{
				float noise = 0.5f + (Mathf.PerlinNoise(x / 15.0f, y / 15.0f) * 0.001f);
				float distFromCenter = Vector2.Distance(new Vector2(0.5f, 0.5f),
														new Vector2(x / (float)heights.GetLength(0),
																	y / (float)heights.GetLength(1)));

				float dropoff = 1.0f - Mathf.Clamp01((distFromCenter - 0.4f) / Mathf.Sqrt(0.5f));
				heights[y, x] = noise * dropoff;
			}
		}

		data.SetHeights(0, 0, heights);
	}
}