using System;
using UnityEngine;

public static class RectExtensions
{
	public static float Distance(this Rect rect, Vector3 point)
	{
		return Mathf.Sqrt(rect.SqrDistance(point));
	}

	public static float SqrDistance(this Rect rect, Vector3 point)
	{
		float cx = point.x - Mathf.Max(Mathf.Min(point.x, rect.x + rect.width ), rect.x);
		float cy = point.y - Mathf.Max(Mathf.Min(point.y, rect.y + rect.height), rect.y);
		return cx*cx + cy*cy;
	}
}