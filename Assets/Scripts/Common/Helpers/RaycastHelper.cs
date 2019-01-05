using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaycastHelper
{
    public static RaycastHit2D ScreenRaycast2D(Vector2 screenPoint, int layerMask)
    {
        return Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(screenPoint), float.PositiveInfinity, layerMask);
    }

    public static RaycastHit2D ScreenRaycast2D(Vector2 screenPoint)
    {
        return ScreenRaycast2D(screenPoint, int.MaxValue);
    }

    public static RaycastHit2D ScreenRaycast2D(int layerMask)
    {
        return ScreenRaycast2D(Input.mousePosition, layerMask);
    }

    public static RaycastHit2D ScreenRaycast2D()
    {
        return ScreenRaycast2D(Input.mousePosition);
    }

    public static bool ScreenRaycast(Vector2 screenPoint, int layerMask, out RaycastHit hit)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
		bool result = Physics.Raycast(ray, out RaycastHit rayHit, float.PositiveInfinity, layerMask);
		hit = rayHit;
        return result;
    }

    public static bool ScreenRaycast(Vector2 screenPoint, out RaycastHit hit)
    {
        return ScreenRaycast(screenPoint, int.MaxValue, out hit);
    }
	
    public static bool ScreenRaycast(out RaycastHit hit)
    {
        return ScreenRaycast(Input.mousePosition, out hit);
    }
	
    public static bool ScreenRaycast(int layerMask, out RaycastHit hit)
    {
        return ScreenRaycast(Input.mousePosition, layerMask, out hit);
    }

    public static Vector3 ScreenRaycastAtPlane(Vector3 screenPoint, Vector3 direction)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        Plane plane = new Plane(direction, Vector3.zero);
		if (plane.Raycast(ray, out float distance))
			return ray.GetPoint(distance);
		return Vector3.zero;
    }

    public static Vector3 ScreenRaycastAtPlane(Vector3 direction)
    {
        return ScreenRaycastAtPlane(Input.mousePosition, direction);
    }

    public static Vector3 ScreenRaycastAtPlane()
    {
        return ScreenRaycastAtPlane(Input.mousePosition, Vector3.forward);
    }
}