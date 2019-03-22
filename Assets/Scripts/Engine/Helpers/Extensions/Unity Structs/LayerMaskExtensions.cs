using UnityEngine;

namespace Engine
{
	public static class LayerMaskExtensions
	{
		public static bool Contains(this LayerMask mask, int layer)
		{
			return mask == (mask | (1 << layer));
		}
	}
}