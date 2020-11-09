using UnityEngine;

namespace Engine
{
	public static class GameObjectExtensions
	{
		/// <summary>
		/// Returns whether a GameObject is a prefab.
		/// </summary>
		public static bool IsPrefab(this GameObject go)
		{
			return go.scene.name == null;
		}
	}
}