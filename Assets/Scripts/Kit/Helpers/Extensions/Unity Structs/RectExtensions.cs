using UnityEngine;

namespace Kit
{
	/// <summary><see cref="Rect" /> extensions.</summary>
	public static class RectExtensions
	{
		/// <summary>Returns the top-left point of a <see cref="Rect" />.</summary>
		public static Vector2 TopLeft(this Rect rect)
		{
			return new Vector2(rect.xMin, rect.yMin);
		}

		/// <summary>Returns the top-right point of a <see cref="Rect" />.</summary>
		public static Vector2 TopRight(this Rect rect)
		{
			return new Vector2(rect.xMin, rect.yMax);
		}

		/// <summary>Returns the bottom-left point of a <see cref="Rect" />.</summary>
		public static Vector2 BottomLeft(this Rect rect)
		{
			return new Vector2(rect.xMax, rect.yMin);
		}

		/// <summary>Returns the bottom-right point of a <see cref="Rect" />.</summary>
		public static Vector2 BottomRight(this Rect rect)
		{
			return new Vector2(rect.xMax, rect.yMax);
		}

		/// <summary>Returns the distance of the rect from a point.</summary>
		public static float Distance(this Rect rect, Vector3 point)
		{
			return Mathf.Sqrt(rect.SqrDistance(point));
		}

		/// <summary>Returns the squared distance of the rect from a point.</summary>
		public static float SqrDistance(this Rect rect, Vector3 point)
		{
			float cx = point.x - Mathf.Max(Mathf.Min(point.x, rect.x + rect.width),  rect.x);
			float cy = point.y - Mathf.Max(Mathf.Min(point.y, rect.y + rect.height), rect.y);
			return cx * cx + cy * cy;
		}
	}
}