using System;
using UnityEngine;
using UnityEngine.UI;

public class IgnoreRaycast : MonoBehaviour, ICanvasRaycastFilter 
{
	public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
	{
		return false;
	}
}
