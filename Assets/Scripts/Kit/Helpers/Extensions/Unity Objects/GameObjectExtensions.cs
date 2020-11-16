using UnityEngine;

namespace Kit
{
	/// <summary><see cref="GameObject" /> extensions.</summary>
	public static class GameObjectExtensions
	{
		/// <summary>Returns whether a <see cref="GameObject" /> is a prefab.</summary>
		public static bool IsPrefab(this GameObject go)
		{
			return go.scene.name == null;
		}
	}
}