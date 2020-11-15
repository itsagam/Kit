using UnityEngine;

namespace Kit
{
	/// <summary>
	/// Helper functions for doing math.
	/// </summary>
	public static class MathHelper
	{
		/// <summary>
		/// Returns whether a vector is within a range of two other vectors.
		/// </summary>
		public static bool IsInRange(Vector2 vector, Vector2 from, Vector2 to)
		{
			return IsInRange(vector.x, from.x, to.x) && IsInRange(vector.y, from.y, to.y);
		}

		/// <summary>
		/// Returns whether a vector is within a range of two other vectors.
		/// </summary>
		public static bool IsInRange(Vector3 vector, Vector3 from, Vector3 to)
		{
			return IsInRange(vector.x, from.x, to.x) &&
				   IsInRange(vector.y, from.y, to.y) &&
				   IsInRange(vector.z, from.z, to.z);
		}

		/// <summary>
		/// <para>Returns whether a number is between two other numbers.</para>
		/// <para>Respects <paramref name="from"/> being larger than <paramref name="to"/>.</para>
		/// </summary>
		public static bool IsInRange(float number, float from, float to)
		{
			float min = from;
			float max = to;
			if (max < min)
			{
				min = to;
				max = from;
			}

			return number >= min && number <= max;
		}

		/// <summary>
		/// <para>Returns whether a number is between two other numbers.</para>
		/// <para>Respects <paramref name="from"/> being larger than <paramref name="to"/>.</para>
		/// </summary>
		public static bool IsInRange(int number, int from, int to)
		{
			int min = from;
			int max = to;
			if (max < min)
			{
				min = to;
				max = from;
			}

			return number >= min && number <= max;
		}

		/// <summary>
		/// Limits a number between two other numbers.
		/// </summary>
		public static float Clamp(float value, float from, float to)
		{
			float min = from;
			float max = to;
			if (max < min)
			{
				min = to;
				max = from;
			}

			return Mathf.Clamp(value, min, max);
		}

		/// <summary>
		/// <para>Limits a number between two other numbers.</para>
		/// <para>Respects <paramref name="from"/> being larger than <paramref name="to"/>.</para>
		/// </summary>
		public static int Clamp(int value, int from, int to)
		{
			int min = from;
			int max = to;
			if (max < min)
			{
				min = to;
				max = from;
			}

			return Mathf.Clamp(value, min, max);
		}

		/// <summary>
		/// Returns what a <paramref name="value"/> would map down to if its minimum and maximum values are changed.
		/// </summary>
		/// <param name="value">The value to map.</param>
		/// <param name="inMin">The input minimum value.</param>
		/// <param name="inMax">The input maximum value.</param>
		/// <param name="outMin">The output minimum value.</param>
		/// <param name="outMax">The output maximum value.</param>
		public static float Map(float value, float inMin, float inMax, float outMin, float outMax)
		{
			return (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
		}

		/// <summary>
		/// Returns the point when a vector is rotated around another one.
		/// </summary>
		/// <param name="point">The point to rotate.</param>
		/// <param name="pivot">The pivot to rotate around.</param>
		/// <param name="angle">The angle to rotate.</param>
		public static Vector3 RotateAround(Vector3 point, Vector3 pivot, Vector3 angle)
		{
			return RotateAround(point, pivot, Quaternion.Euler(angle));
		}

		/// <summary>
		/// Returns the point when a vector is rotated around another one.
		/// </summary>
		/// <param name="point">The point to rotate.</param>
		/// <param name="pivot">The pivot to rotate around.</param>
		/// <param name="angle">The angle to rotate.</param>
		public static Vector3 RotateAround(Vector3 point, Vector3 pivot, Quaternion angle)
		{
			return angle * (point - pivot) + pivot;
		}

		/// <summary>
		/// Returns the point at a certain distance from a another point.
		/// </summary>
		/// <param name="destination">The target point.</param>
		/// <param name="origin">Current position.</param>
		/// <param name="distance">Distance to maintain.</param>
		public static Vector3 GetPositionAtDistance(Vector3 destination, Vector3 origin, float distance)
		{
			Vector3 direction = (destination - origin).normalized;
			return destination - direction * distance;
		}

		/// <summary>
		/// Returns at what distance a camera should be to keep a certain area in view.
		/// </summary>
		/// <param name="from">The camera to use.</param>
		/// <param name="radius">Radius of the area.</param>
		public static float DistanceToView(Camera from, float radius)
		{
			return radius / Mathf.Sin(from.fieldOfView * Mathf.Deg2Rad / 2f);
		}

		/// <summary>
		/// Returns the angle between two vectors.
		/// </summary>
		public static float AngleBetween(Vector2 a, Vector2 b)
		{
			Vector2 direction = b - a;
			return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		}

		/// <summary>
		/// Rotates a point at a given angle.
		/// </summary>
		public static Vector2 Rotate(Vector2 point, float angle)
		{
			float rad = angle * Mathf.Deg2Rad;
			float cos = Mathf.Cos(rad);
			float sin = Mathf.Sin(rad);
			return new Vector2(point.x * cos - point.y * sin, point.x * sin + point.y * cos);
		}

		/// <summary>
		/// Loop an angle around within a -180° to +180° range.
		/// </summary>
		public static float ClampDeltaAngle(float delta)
		{
			if (delta > 180)
				delta = 360 - delta;
			else if (delta < -180)
				delta += 360;
			return delta;
		}

		/// <summary>
		/// Loop an angle around within a -360° to +360° range.
		/// </summary>
		public static float ClampAngle(float angle)
		{
			if (angle < -360)
				angle += 360;
			else if (angle > 360)
				angle -= 360;
			return angle;
		}

		/// <summary>
		/// <para>Loop an angle around within a -360° to +360° range and limit it between two values.</para>
		/// <para>Respects <paramref name="from"/> being larger than <paramref name="to"/>.</para>
		/// </summary>
		public static float ClampAngle(float angle, float from, float to)
		{
			return Clamp(ClampAngle(angle), from, to);
		}
	}
}