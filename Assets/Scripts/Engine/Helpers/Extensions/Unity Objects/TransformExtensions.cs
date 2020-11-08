using UnityEngine;

namespace Engine
{
	public static class TransformExtensions
	{
		public static Bounds GetBounds(this Transform transform)
		{
			Bounds result = new Bounds { center = transform.position };
			var renderers = transform.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in renderers)
				result.Encapsulate(renderer.bounds);
			return result;
		}

		public static bool IsFirstSibling(this Transform transform)
		{
			return transform.GetSiblingIndex() == 0;
		}

		public static bool IsLastSibling(this Transform transform)
		{
			return transform.GetSiblingIndex() == transform.parent.childCount - 1;
		}

		public static Transform GetFirstChild(this Transform transform)
		{
			return transform.GetChild(0);
		}

		public static Transform GetLastChild(this Transform transform)
		{
			return transform.GetChild(transform.childCount - 1);
		}

		public static void Face(this Transform transform, Transform other)
		{
			transform.rotation = Quaternion.LookRotation(other.position - transform.position);
		}

		public static void Face(this Transform transform, Vector3 position)
		{
			transform.rotation = Quaternion.LookRotation(position - transform.position);
		}
	}
}