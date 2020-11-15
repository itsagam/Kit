using UnityEngine;

namespace Kit
{
	/// <summary>
	/// Helper functions for ray-casting.
	/// </summary>
	public static class RaycastHelper
	{
		/// <summary>
		/// Cast a 2D ray from the mouse position.
		/// </summary>
		public static RaycastHit2D ScreenRaycast2D(Camera camera, int layerMask = -5)
		{
			return ScreenRaycast2D(camera, Input.mousePosition, layerMask);
		}

		/// <summary>
		/// Cast a 2D ray from a certain point on the screen.
		/// </summary>
		public static RaycastHit2D ScreenRaycast2D(Camera camera, Vector2 screenPoint, int layerMask = -5)
		{
			return Physics2D.GetRayIntersection(camera.ScreenPointToRay(screenPoint), float.PositiveInfinity, layerMask);
		}

		/// <summary>
		/// Cast a ray from the mouse position.
		/// </summary>
		public static bool ScreenRaycast(Camera camera, out RaycastHit hit, int layerMask = -5)
		{
			return ScreenRaycast(camera, Input.mousePosition, out hit, layerMask);
		}

		/// <summary>
		/// Cast a ray from a certain point on the screen.
		/// </summary>
		public static bool ScreenRaycast(Camera camera, Vector2 screenPoint, out RaycastHit hit, int layerMask = -5)
		{
			Ray ray = camera.ScreenPointToRay(screenPoint);
			bool result = Physics.Raycast(ray, out RaycastHit rayHit, float.PositiveInfinity, layerMask);
			hit = rayHit;
			return result;
		}

		/// <summary>
		/// Cast a ray from the mouse position towards a plane.
		/// </summary>
		public static Vector3? ScreenRaycastAtPlane(Camera camera, Plane plane)
		{
			return ScreenRaycastAtPlane(camera, Input.mousePosition, plane);
		}

		/// <summary>
		/// Cast a ray from a certain point on the screen towards a plane.
		/// </summary>
		public static Vector3? ScreenRaycastAtPlane(Camera camera, Vector3 screenPoint, Plane plane)
		{
			Ray ray = camera.ScreenPointToRay(screenPoint);
			if (plane.Raycast(ray, out float distance))
				return ray.GetPoint(distance);
			return null;
		}
	}
}