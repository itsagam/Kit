﻿using System;
using UnityEngine;

public static class TransformExtensions
{
	public static Bounds GetBounds(this Transform transform)
	{
		Bounds result = new Bounds();
		result.center = transform.position;
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

	public static bool IsFirstSibling(this Transform transform)
	{
		return transform.GetSiblingIndex() == 0;
	}

	public static bool IsLastSibling(this Transform transform)
	{
		return transform.GetSiblingIndex() == transform.parent.childCount - 1;
	}

	public static Transform GetFirstChild(this Transform transform)
	{
		return transform.GetChild(0);
	}

	public static Transform GetLastChild(this Transform transform)
	{
		return transform.GetChild(transform.childCount - 1);
	}
}	