using System.Linq;
using UnityEngine;

namespace Kit
{
	/// <summary>
	/// <see cref="Bounds"/> extensions.
	/// </summary>
	public static class BoundsExtensions
	{
		/// <summary>
		/// Rotate the <see cref="Bounds"/>.
		/// </summary>
		public static Bounds Rotate(this Bounds bounds, Quaternion rotation)
		{
			var points = bounds.GetPoints();
			for (int i = 0; i < points.Length; i++)
				points[i] = rotation * points[i];
			Bounds rotated = new Bounds
							 {
								 min = new Vector3(points.Min(v => v.x), points.Min(v => v.y), points.Min(v => v.z)),
								 max = new Vector3(points.Max(v => v.x), points.Max(v => v.y), points.Max(v => v.z))
							 };
			return rotated;
		}

		/// <summary>
		/// Get all the vertices on the <see cref="Bounds"/>.
		/// </summary>
		public static Vector3[] GetPoints(this Bounds bounds)
		{
			var points = new Vector3[8];
			points[0] = bounds.min;
			points[1] = bounds.max;
			points[2] = new Vector3(points[0].x, points[0].y, points[1].z);
			points[3] = new Vector3(points[0].x, points[1].y, points[0].z);
			points[4] = new Vector3(points[1].x, points[0].y, points[0].z);
			points[5] = new Vector3(points[0].x, points[1].y, points[1].z);
			points[6] = new Vector3(points[1].x, points[0].y, points[1].z);
			points[7] = new Vector3(points[1].x, points[1].y, points[0].z);
			return points;
		}

		/// <summary>
		/// Get a random point within the <see cref="Bounds"/>.
		/// </summary>
		public static Vector3 GetRandomPoint(this Bounds bounds)
		{
			Vector3 min = bounds.min;
			Vector3 max = bounds.max;
			return new Vector3(Random.Range(min.x, max.x),
							   Random.Range(min.y, max.y),
							   Random.Range(min.z, max.z));
		}

		/// <summary>
		/// Returns whether it overlaps with another <see cref="Bounds"/>.
		/// </summary>
		public static bool Overlaps(this Bounds boundsA, Bounds boundsB)
		{
			Vector3 topLeftA = boundsA.min, bottomRightA = boundsA.max;
			Vector3 topLeftB = boundsB.min, bottomRightB = boundsB.max;
			return topLeftA.x     < bottomRightB.x &&
				   bottomRightA.x > topLeftB.x     &&
				   topLeftA.y     < bottomRightB.y &&
				   bottomRightA.y > topLeftB.y     &&
				   topLeftA.z     < bottomRightB.z &&
				   bottomRightA.z > topLeftB.z;
		}

		/// <summary>
		/// Returns the distance to a point.
		/// </summary>
		public static float GetDistance(this Bounds bounds, Vector3 point)
		{
			return Mathf.Sqrt(bounds.SqrDistance(point));
		}
	}
}