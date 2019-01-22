using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BoundsExtensions
{
    public static Bounds Rotate(this Bounds bounds, Quaternion rotation)
    {
        Vector3[] points = bounds.Points();
        for (int i = 0; i < points.Length; i++)
            points[i] = rotation * points[i];
        Bounds rotated = new Bounds();
        rotated.min = new Vector3(points.Min(v => v.x), points.Min(v => v.y), points.Min(v => v.z));
        rotated.max = new Vector3(points.Max(v => v.x), points.Max(v => v.y), points.Max(v => v.z));
        return rotated;
    }

    public static Vector3[] Points(this Bounds bounds)
    {
        Vector3[] points = new Vector3[8];
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

    public static Vector3 Random(this Bounds bounds)
    {
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        return new Vector3(UnityEngine.Random.Range(min.x, max.x), UnityEngine.Random.Range(min.y, max.y), UnityEngine.Random.Range(min.z, max.z));
    }

    public static bool Overlaps(this Bounds boundsA, Bounds boundsB)
    {
        Vector3 topLeftA = boundsA.min, bottomRightA = boundsA.max;
        Vector3 topLeftB = boundsB.min, bottomRightB = boundsB.max;
        if (topLeftA.x < bottomRightB.x && bottomRightA.x > topLeftB.x &&
        topLeftA.y < bottomRightB.y && bottomRightA.y > topLeftB.y &&
        topLeftA.z < bottomRightB.z && bottomRightA.z > topLeftB.z)
            return true;
        return false;
    }

    public static float Distance(this Bounds bounds, Vector3 point)
    {
        return Mathf.Sqrt(bounds.SqrDistance(point));
    }
}