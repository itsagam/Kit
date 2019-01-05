using System;
using UnityEngine;

public static class Vector3Extensions
{
	public static Vector3 Copy(this Vector3 vector)
	{
		return new Vector3(vector.x, vector.y, vector.z);
	}

	public static Vector3 CopyX(this Vector3 vector, Vector3 from)
	{
		vector.x = from.x;
		return vector;
	}

	public static Vector3 CopyY(this Vector3 vector, Vector3 from)
	{
		vector.y = from.y;
		return vector;
	}

	public static Vector3 CopyZ(this Vector3 vector, Vector3 from)
	{
		vector.z = from.z;
		return vector;
	}
	
	public static Vector3 CopyXY(this Vector3 vector, Vector3 from)
	{
		vector.x = from.x;
		vector.y = from.y;
		return vector;
	}
	
	public static Vector3 CopyXZ(this Vector3 vector, Vector3 from)
	{
		vector.x = from.x;
		vector.z = from.z;
		return vector;
	}
	
	public static Vector3 CopyYZ(this Vector3 vector, Vector3 from)
	{
		vector.y = from.y;
		vector.z = from.z;
		return vector;
	}

	public static Vector3 AddX(this Vector3 vector, float x)
	{
		vector.x += x;
		return vector;
	}
	
	public static Vector3 AddY(this Vector3 vector, float y)
	{
		vector.y += y;
		return vector;
	}
	
	public static Vector3 AddZ(this Vector3 vector, float z)
	{
		vector.z += z;
		return vector;
	}
	
	public static Vector3 Scale(this Vector3 vector, float x, float y, float z)
	{
		vector.x *= x;
		vector.y *= y;
		vector.z *= z;
		return vector;
	}

	public static Vector3 ScaleX(this Vector3 vector, float x)
	{	
		vector.x *= x;
		return vector;
	}
	
	public static Vector3 ScaleY(this Vector3 vector, float y)
	{	
		vector.y *= y;
		return vector;
	}
	
	public static Vector3 ScaleZ(this Vector3 vector, float z)
	{
		vector.z *= z;
		return vector;
	}

	public static Vector3 SetX(this Vector3 vector, float x)
	{
		vector.x = x;
		return vector;
	}

	public static Vector3 SetY(this Vector3 vector, float y)
	{
		vector.y = y;
		return vector;
	}
	
	public static Vector3 SetZ(this Vector3 vector, float z)
	{
		vector.z = z;
		return vector;
	}

	public static Vector3 SwapXY(this Vector3 vector)
	{
		return new Vector3(vector.y, vector.x, vector.z);
	}

	public static Vector3 SwapYZ(this Vector3 vector)
	{
		return new Vector3(vector.x, vector.z, vector.y);
	}

	public static Vector3 SwapXZ(this Vector3 vector)
	{
		return new Vector3(vector.z, vector.y, vector.x);
	}

	public static Vector3 InvertX(this Vector3 vector)
	{
		vector.x = -vector.x;
		return vector;
	}
	
	public static Vector3 InvertY(this Vector3 vector)
	{
		vector.y = -vector.y;
		return vector;
	}
	
	public static Vector3 InvertZ(this Vector3 vector)
	{
		vector.z = -vector.z;
		return vector;
	}

	public static float Min(this Vector3 vector)
	{
		float min = vector.x < vector.y ? vector.x : vector.y;
		return min < vector.z ? min : vector.z;
	}

	public static float MinXY(this Vector3 vector)
	{
		return vector.x < vector.y ? vector.x : vector.y;
	}

	public static float MinYZ(this Vector3 vector)
	{
		return vector.y < vector.z ? vector.y : vector.z;
	}

	public static float MinXZ(this Vector3 vector)
	{
		return vector.x < vector.z ? vector.x : vector.z;
	}

	public static float Max(this Vector3 vector)
	{
		float max = vector.x > vector.y ? vector.x : vector.y;
		return max > vector.z ? max : vector.z;
	}

	public static float MaxXY(this Vector3 vector)
	{
		return vector.x > vector.y ? vector.x : vector.y;
	}

	public static float MaxYZ(this Vector3 vector)
	{
		return vector.y > vector.z ? vector.y : vector.z;
	}

	public static float MaxXZ(this Vector3 vector)
	{
		return vector.x > vector.z ? vector.x : vector.z;
	}

	public static Quaternion ToQuaternion(this Vector3 vector)
	{
		return Quaternion.Euler(vector);
	}

	public static Vector2 ToVector2(this Vector3 vector)
	{
		return vector;
	}
}