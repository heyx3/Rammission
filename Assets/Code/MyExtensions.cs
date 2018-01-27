﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public static class MyExtensions
{
	public static Vector2 Horz(this Vector3 v) { return new Vector2(v.x, v.z); }
	public static Vector3 To3D(this Vector2 v, float height = 0.0f) { return new Vector3(v.x, height, v.y); }

	public static int IndexOf<T>(this IEnumerable<T> items, Predicate<T> pred)
	{
		int i = 0;
		foreach (var item in items)
		{
			if (pred(item))
				return i;
			i += 1;
		}

		return -1;
	}
}