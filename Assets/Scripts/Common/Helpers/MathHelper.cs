using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MathHelper
{
    public static bool IsInRange(Vector2 number, Vector2 from, Vector2 to)
    {
        return IsInRange(number.x, from.x, to.x) && IsInRange(number.y, from.y, to.y);
    }

    public static bool IsInRange(Vector3 number, Vector3 from, Vector3 to)
    {
        return IsInRange(number.x, from.x, to.x) && IsInRange(number.y, from.y, to.y) && IsInRange(number.z, from.z, to.z);
    }

    public static bool IsInRange(float number, float from, float to)
    {
        float min = from;
        float max = to;
        if (max < min)
        {
            min = to;
            max = from;
        }
        return (number >= min && number <= max);
    }

    public static bool IsInRange(int number, int from, int to)
    {
        int min = from;
        int max = to;
        if (max < min)
        {
            min = to;
            max = from;
        }
        return (number >= min && number <= max);
    }

	public static float Map(float value, float inMin, float inMax, float outMin, float outMax)
	{
		return (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
	}

	public static Vector3 RotateAround(Vector3 point, Vector3 pivot, Vector3 angle)
    {
        return RotateAround(point, pivot, Quaternion.Euler(angle));
    }

    public static Vector3 RotateAround(Vector3 point, Vector3 pivot, Quaternion angle)
    {
        return angle * (point - pivot) + pivot;
    }

    public static Vector3 GetPositionAtDistance(Vector3 destination, Vector3 origin, float distance)
    {
        Vector3 direction = (destination - origin).normalized;
        return destination - (direction * distance);
    }

    public static float DistanceToView(Camera from, float radius)
    {
        return radius / Mathf.Sin(from.fieldOfView * Mathf.Deg2Rad / 2f);
    }

    public static float AngleBetween(Vector2 a, Vector2 b)
    {
        Vector2 direction = b - a;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    public static Vector2 Rotate(Vector2 point, float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(point.x * cos - point.y * sin, point.x * sin + point.y * cos);
    }

    public static float ClampDeltaAngle(float delta)
    {
        if (delta > 180)
            delta = 360 - delta;
        else if (delta < -180)
            delta = delta + 360;
        return delta;
    }

    public static float ClampAngle(float angle)
    {
        if (angle < -360)
            angle += 360;
        else if (angle > 360)
            angle -= 360;
        return angle;
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        return Mathf.Clamp(ClampAngle(angle), min, max);
    }
}