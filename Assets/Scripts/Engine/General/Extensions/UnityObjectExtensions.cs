using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnityObjectExtensions
{
	public static void Destroy(this UnityEngine.Object obj)
	{
		GameObject.Destroy(obj);
	}
}