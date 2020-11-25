using UnityEngine;
using XLua;

namespace Kit
{
	/// <summary><see cref="Vector3" /> extensions.</summary>
	public static class Vector3Extensions
	{
		/// <summary>Copy values from another instance and return the vector.</summary>
		public static Vector3 Copy(this Vector3 vector)
		{
			return new Vector3(vector.x, vector.y, vector.z);
		}

		/// <summary>Copy the X-component and return the vector.</summary>
		public static Vector3 CopyX(this Vector3 vector, Vector3 from)
		{
			vector.x = from.x;
			return vector;
		}

		/// <summary>Copy the Y-component and return the vector.</summary>
		public static Vector3 CopyY(this Vector3 vector, Vector3 from)
		{
			vector.y = from.y;
			return vector;
		}

		/// <summary>Copy the Z-component and return the vector.</summary>
		public static Vector3 CopyZ(this Vector3 vector, Vector3 from)
		{
			vector.z = from.z;
			return vector;
		}

		/// <summary>Copy the X &amp; Y components and return the vector.</summary>
		public static Vector3 CopyXY(this Vector3 vector, Vector3 from)
		{
			vector.x = from.x;
			vector.y = from.y;
			return vector;
		}

		/// <summary>Copy the X &amp; Z components and return the vector.</summary>
		public static Vector3 CopyXZ(this Vector3 vector, Vector3 from)
		{
			vector.x = from.x;
			vector.z = from.z;
			return vector;
		}

		/// <summary>Copy the Y &amp; Z components and return the vector.</summary>
		public static Vector3 CopyYZ(this Vector3 vector, Vector3 from)
		{
			vector.y = from.y;
			vector.z = from.z;
			return vector;
		}

		/// <summary>Add to the X-component and return the vector.</summary>
		public static Vector3 AddX(this Vector3 vector, float x)
		{
			vector.x += x;
			return vector;
		}

		/// <summary>Add to the Y-component and return the vector.</summary>
		public static Vector3 AddY(this Vector3 vector, float y)
		{
			vector.y += y;
			return vector;
		}

		/// <summary>Add to the Z-component and return the vector.</summary>
		public static Vector3 AddZ(this Vector3 vector, float z)
		{
			vector.z += z;
			return vector;
		}

		/// <summary>Scale the vector and return it.</summary>
		public static Vector3 Scale(this Vector3 vector, float x, float y, float z)
		{
			vector.x *= x;
			vector.y *= y;
			vector.z *= z;
			return vector;
		}

		/// <summary>Scale the X-component and return the vector.</summary>
		public static Vector3 ScaleX(this Vector3 vector, float x)
		{
			vector.x *= x;
			return vector;
		}

		/// <summary>Scale the Y-component and return the vector.</summary>
		public static Vector3 ScaleY(this Vector3 vector, float y)
		{
			vector.y *= y;
			return vector;
		}

		/// <summary>Scale the Z-component and return the vector.</summary>
		public static Vector3 ScaleZ(this Vector3 vector, float z)
		{
			vector.z *= z;
			return vector;
		}

		/// <summary>Set the X-component and return the vector.</summary>
		public static Vector3 SetX(this Vector3 vector, float x)
		{
			vector.x = x;
			return vector;
		}

		/// <summary>Set the Y-component and return the vector.</summary>
		public static Vector3 SetY(this Vector3 vector, float y)
		{
			vector.y = y;
			return vector;
		}

		/// <summary>Set the Z-component and return the vector.</summary>
		public static Vector3 SetZ(this Vector3 vector, float z)
		{
			vector.z = z;
			return vector;
		}

		/// <summary>Swap the X &amp; Y components and return the vector.</summary>
		public static Vector3 SwapXY(this Vector3 vector)
		{
			return new Vector3(vector.y, vector.x, vector.z);
		}

		/// <summary>Swap the Y &amp; Z components and return the vector.</summary>
		public static Vector3 SwapYZ(this Vector3 vector)
		{
			return new Vector3(vector.x, vector.z, vector.y);
		}

		/// <summary>Swap the X &amp; Z components and return the vector.</summary>
		public static Vector3 SwapXZ(this Vector3 vector)
		{
			return new Vector3(vector.z, vector.y, vector.x);
		}

		/// <summary>Invert the X-component and return the vector.</summary>
		public static Vector3 InvertX(this Vector3 vector)
		{
			vector.x = -vector.x;
			return vector;
		}

		/// <summary>Invert the Y-component and return the vector.</summary>
		public static Vector3 InvertY(this Vector3 vector)
		{
			vector.y = -vector.y;
			return vector;
		}

		/// <summary>Invert the Z-component and return the vector.</summary>
		public static Vector3 InvertZ(this Vector3 vector)
		{
			vector.z = -vector.z;
			return vector;
		}

		/// <summary>Return the smallest of the components.</summary>
		public static float Min(this Vector3 vector)
		{
			float min = vector.x < vector.y ? vector.x : vector.y;
			return min < vector.z ? min : vector.z;
		}

		/// <summary>Return the smaller of the X &amp; Y components.</summary>
		public static float MinXY(this Vector3 vector)
		{
			return vector.x < vector.y ? vector.x : vector.y;
		}

		/// <summary>Return the smaller of the Y &amp; Z components.</summary>
		public static float MinYZ(this Vector3 vector)
		{
			return vector.y < vector.z ? vector.y : vector.z;
		}

		/// <summary>Return the smaller of the X &amp; Z components.</summary>
		public static float MinXZ(this Vector3 vector)
		{
			return vector.x < vector.z ? vector.x : vector.z;
		}

		/// <summary>Return the largest of the components.</summary>
		public static float Max(this Vector3 vector)
		{
			float max = vector.x > vector.y ? vector.x : vector.y;
			return max > vector.z ? max : vector.z;
		}

		/// <summary>Return the larger of the X &amp; Y components.</summary>
		public static float MaxXY(this Vector3 vector)
		{
			return vector.x > vector.y ? vector.x : vector.y;
		}

		/// <summary>Return the larger of the Y &amp; Z components.</summary>
		public static float MaxYZ(this Vector3 vector)
		{
			return vector.y > vector.z ? vector.y : vector.z;
		}

		/// <summary>Return the larger of the X &amp; Z components.</summary>
		public static float MaxXZ(this Vector3 vector)
		{
			return vector.x > vector.z ? vector.x : vector.z;
		}

		/// <summary>Limit the vector between two other vectors.</summary>
		public static Vector3 Clamp(this Vector3 vector, Vector3 min, Vector3 max)
		{
			return new Vector3(MathHelper.Clamp(vector.x, min.x, max.x),
							   MathHelper.Clamp(vector.y, min.y, max.y),
							   MathHelper.Clamp(vector.z, min.z, max.z));
		}

		/// <summary>Make all components positive.</summary>
		public static Vector3 Abs(this Vector3 vector)
		{
			return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
		}

		/// <summary>Make a rotation with the vector as euler angles.</summary>
		public static Quaternion ToQuaternion(this Vector3 vector)
		{
			return Quaternion.Euler(vector);
		}

		/// <summary>Convert to Vector2.</summary>
		public static Vector2 ToVector2(this Vector3 vector)
		{
			return vector;
		}
	}
}