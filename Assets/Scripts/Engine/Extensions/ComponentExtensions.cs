using UnityEngine;

namespace Engine
{
	public static class ComponentExtensions
	{
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