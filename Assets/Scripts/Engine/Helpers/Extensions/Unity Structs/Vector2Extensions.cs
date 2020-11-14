using UnityEngine;

namespace Engine
{
	/// <summary>
	/// <see cref="Vector2"/> extensions.
	/// </summary>
	public static class Vector2Extensions
	{
		/// <summary>
		/// Copy values from another instance and return the vector.
		/// </summary>
		public static Vector2 Copy(this Vector2 vector)
		{
			return new Vector2(vector.x, vector.y);
		}

		/// <summary>
		/// Copy the X-component and return the vector.
		/// </summary>
		public static Vector2 CopyX(this Vector2 vector, Vector2 from)
		{
			vector.x = from.x;
			return vector;
		}

		/// <summary>
		/// Copy the Y-component and return the vector.
		/// </summary>
		public static Vector2 CopyY(this Vector2 vector, Vector2 from)
		{
			vector.y = from.y;
			return vector;
		}

		/// <summary>
		/// Add to the X-component and return the vector.
		/// </summary>
		public static Vector2 AddX(this Vector2 vector, float x)
		{
			vector.x += x;
			return vector;
		}

		/// <summary>
		/// Add to the Y-component and return the vector.
		/// </summary>
		public static Vector2 AddY(this Vector2 vector, float y)
		{
			vector.y += y;
			return vector;
		}

		/// <summary>
		/// Scale the vector and return it.
		/// </summary>
		public static Vector2 Scale(this Vector2 vector, float x, float y)
		{
			vector.x *= x;
			vector.y *= y;
			return vector;
		}

		/// <summary>
		/// Scale the X-component and return the vector.
		/// </summary>
		public static Vector2 ScaleX(this Vector2 vector, float x)
		{
			vector.x *= x;
			return vector;
		}

		/// <summary>
		/// Scale the Y-component and return the vector.
		/// </summary>
		public static Vector2 ScaleY(this Vector2 vector, float y)
		{
			vector.y *= y;
			return vector;
		}

		/// <summary>
		/// Set the X-component and return the vector.
		/// </summary>
		public static Vector2 SetX(this Vector2 vector, float x)
		{
			vector.x = x;
			return vector;
		}

		/// <summary>
		/// Set the Y-component and return the vector.
		/// </summary>
		public static Vector2 SetY(this Vector2 vector, float y)
		{
			vector.y = y;
			return vector;
		}

		/// <summary>
		/// Swap the X &amp; Y components and return the vector.
		/// </summary>
		public static Vector2 Swap(this Vector2 vector)
		{
			return new Vector2(vector.y, vector.x);
		}

		/// <summary>
		/// Invert the X-component and return the vector.
		/// </summary>
		public static Vector2 InvertX(this Vector2 vector)
		{
			vector.x = -vector.x;
			return vector;
		}

		/// <summary>
		/// Invert the Y-component and return the vector.
		/// </summary>
		public static Vector2 InvertY(this Vector2 vector)
		{
			vector.y = -vector.y;
			return vector;
		}

		/// <summary>
		/// Return the smaller of the two components.
		/// </summary>
		public static float Min(this Vector2 vector)
		{
			return vector.x < vector.y ? vector.x : vector.y;
		}

		/// <summary>
		/// Return the larger of the two components.
		/// </summary>
		public static float Max(this Vector2 vector)
		{
			return vector.x > vector.y ? vector.x : vector.y;
		}

		/// <summary>
		/// Limit the vector between two other vectors.
		/// </summary>
		public static Vector2 Clamp(this Vector2 vector, Vector2 min, Vector2 max)
		{
			return new Vector2(Mathf.Clamp(vector.x, min.x, max.x),
							   Mathf.Clamp(vector.y, min.y, max.y));
		}

		/// <summary>
		/// Make all components positive.
		/// </summary>
		public static Vector2 Abs(this Vector2 vector)
		{
			return new Vector2(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
		}

		/// <summary>
		/// Make a rotation with the vector as euler angles.
		/// </summary>
		public static Quaternion ToQuaternion(this Vector2 vector)
		{
			return Quaternion.Euler(vector);
		}

		/// <summary>
		/// Convert to Vector3.
		/// </summary>
		public static Vector3 ToVector3(this Vector2 vector)
		{
			return vector;
		}
	}
}