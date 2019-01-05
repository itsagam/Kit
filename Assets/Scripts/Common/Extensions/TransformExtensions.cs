using System;
using UnityEngine;

public static class TransformHelper
{
	public static Bounds GetBounds(this Transform transform)
	{
		Bounds result = new Bounds();
		Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in renderers)
			result.Encapsulate(renderer.bounds);
		return result;
	}

	public static void CopyLocal(this Transform transform, Transform from)
	{
		transform.localPosition = from.transform.localPosition;
		transform.localRotation = from.transform.localRotation;
		transform.localScale = from.transform.localScale;
	}
}	