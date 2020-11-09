using UnityEngine;

namespace Engine
{
	public static class ComponentExtensions
	{
		/// <summary>
		/// Returns whether the component is a part of a prefab.
		/// </summary>
		public static bool IsPrefab(this Component component)
		{
			return component.gameObject.IsPrefab();
		}

		/// <summary>
		/// Returns the bounds of the component.
		/// </summary>
		/// <remarks>Works directly for Renderers and Colliders, otherwise returns the bounds of the transform.</remarks>
		public static Bounds GetBounds(this Component component)
		{
			switch (component)
			{
				case Transform t:
					return t.GetBounds();

				case Renderer r:
					return r.bounds;

				case Collider c:
					return c.bounds;

				default:
					return component.transform.GetBounds();
			}
		}
	}
}