using UnityEngine;

namespace Engine
{
    public static class RaycastHelper
    {
        public static RaycastHit2D ScreenRaycast2D(Camera camera, Vector2 screenPoint, int layerMask = -5)
        {
            return Physics2D.GetRayIntersection(camera.ScreenPointToRay(screenPoint), float.PositiveInfinity, layerMask);
        }

        public static RaycastHit2D ScreenRaycast2D(Camera camera, int layerMask = -5)
        {
            return ScreenRaycast2D(camera, Input.mousePosition, layerMask);
        }


        public static bool ScreenRaycast(Camera camera, Vector2 screenPoint, out RaycastHit hit, int layerMask = -5)
        {
            Ray ray = camera.ScreenPointToRay(screenPoint);
            bool result = Physics.Raycast(ray, out RaycastHit rayHit, float.PositiveInfinity, layerMask);
            hit = rayHit;
            return result;
        }

        public static bool ScreenRaycast(Camera camera, out RaycastHit hit, int layerMask = -5)
        {
            return ScreenRaycast(camera, Input.mousePosition, out hit, layerMask);
        }


        public static Vector3 ScreenRaycastAtPlane(Camera camera, Vector3 screenPoint, Vector3 direction)
        {
            Ray ray = camera.ScreenPointToRay(screenPoint);
            Plane plane = new Plane(direction, Vector3.zero);
            return plane.Raycast(ray, out float distance) ? ray.GetPoint(distance) : Vector3.zero;
        }

        public static Vector3 ScreenRaycastAtPlane(Camera camera, Vector3? direction = null)
        {
            return ScreenRaycastAtPlane(camera, Input.mousePosition, direction ?? Vector3.forward);
        }
    }
}