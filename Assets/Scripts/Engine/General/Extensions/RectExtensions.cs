using UnityEngine;

public static class RectExtensions
{
	public static Vector2 TopLeft(this Rect rect)
	{
		return new Vector2(rect.xMin, rect.yMin);
	}

	public static Vector2 TopRight(this Rect rect)
	{
		return new Vector2(rect.xMin, rect.yMax);
	}

	public static Vector2 BottomLeft(this Rect rect)
	{
		return new Vector2(rect.xMax, rect.yMin);
	}

	public static Vector2 BottomRight(this Rect rect)
	{
		return new Vector2(rect.xMax, rect.yMax);
	}

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