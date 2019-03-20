using UnityEngine;

namespace Engine
{
	public static class GameObjectExtensions
	{
		public static bool IsPrefab(this GameObject go)
		{
			return go.scene.name == null;
		}
	}
}