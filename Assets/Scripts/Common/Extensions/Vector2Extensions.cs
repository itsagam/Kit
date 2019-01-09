using System;
using UnityEngine;

public static class Vector2Extensions
{
	public static Vector2 Copy(this Vector2 vector)
	{
		return new Vector2(vector.x, vector.y);
	}

	public static Vector2 CopyX(this Vector2 vector, Vector2 from)
	{
		vector.x = from.x;
		return vector;
	}

	public static Vector2 CopyY(this Vector2 vector, Vector2 from)
	{
		vector.y = from.y;
		return vector;
	}

	public static Vector2 AddX(this Vector2 vector, float x)
	{
		vector.x += x;
		return vector;
	}
	
	public static Vector2 AddY(this Vector2 vector, float y)
	{
		vector.y += y;
		return vector;
	}
	
	public static Vector2 Scale(this Vector2 vector, float x, float y, float z)
	{
		vector.x *= x;
		vector.y *= y;
		return vector;
	}

	public static Vector2 ScaleX(this Vector2 vector, float x)
	{	
		vector.x *= x;
		return vector;
	}
	
	public static Vector2 ScaleY(this Vector2 vector, float y)
	{	
		vector.y *= y;
		return vector;
	}

	public static Vector2 SetX(this Vector2 vector, float x)
	{
		vector.x = x;
		return vector;
	}

	public static Vector2 SetY(this Vector2 vector, float y)
	{
		vector.y = y;
		return vector;
	}

	public static Vector2 Swap(this Vector2 vector)
	{
		return new Vector2(vector.y, vector.x);
	}

	public static Vector2 InvertX(this Vector2 vector)
	{
		vector.x = -vector.x;
		return vector;
	}
	
	public static Vector2 InvertY(this Vector2 vector)
	{
		vector.y = -vector.y;
		return vector;
	}

	public static float Min(this Vector2 vector)
	{
		return vector.x < vector.y ? vector.x : vector.y;
	}

	public static float Max(this Vector2 vector)
	{
		return vector.x > vector.y ? vector.x : vector.y;
	}

	public static Vector2 Abs(this Vector2 vector)
	{
		return new Vector2(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
	}

	public static Quaternion ToQuaternion(this Vector2 vector)
	{
		return Quaternion.Euler(vector);
	}

	public static Vector3 ToVector3(this Vector2 vector)
	{
		return vector;
	}
}