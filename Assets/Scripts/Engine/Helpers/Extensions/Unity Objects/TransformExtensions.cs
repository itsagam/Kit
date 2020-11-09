using UnityEngine;

namespace Engine
{
	public static class TransformExtensions
	{
		/// <summary>
		/// Returns the combined bounds of all the renderers attached to this transform.
		/// </summary>
		public static Bounds GetBounds(this Transform transform)
		{
			Bounds result = new Bounds { center = transform.position };
			var renderers = transform.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in renderers)
				result.Encapsulate(renderer.bounds);
			return result;
		}

		/// <summary>
		/// Returns whether the transform is the first one on its parent.
		/// </summary>
		public static bool IsFirstSibling(this Transform transform)
		{
			return transform.GetSiblingIndex() == 0;
		}

		/// <summary>
		/// Returns whether the transform is the last one on its parent.
		/// </summary>
		public static bool IsLastSibling(this Transform transform)
		{
			return transform.GetSiblingIndex() == transform.parent.childCount - 1;
		}

		/// <summary>
		/// Returns the first child of the transform.
		/// </summary>
		public static Transform GetFirstChild(this Transform transform)
		{
			return transform.GetChild(0);
		}

		/// <summary>
		/// Returns the last child of the transform.
		/// </summary>
		public static Transform GetLastChild(this Transform transform)
		{
			return transform.GetChild(transform.childCount - 1);
		}

		/// <summary>
		/// Face another transform.
		/// </summary>
		public static void Face(this Transform transform, Transform other)
		{
			Face(transform, other.position);
		}

		/// <summary>
		/// Face a position.
		/// </summary>
		public static void Face(this Transform transform, Vector3 position)
		{
			transform.rotation = Quaternion.LookRotation(position - transform.position);
		}
	}
}