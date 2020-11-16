using UnityEngine;

namespace Kit
{
	/// <summary><see cref="LayerMask" /> extensions.</summary>
	public static class LayerMaskExtensions
	{
		/// <summary>Returns whether the mask contains a particular layer.</summary>
		public static bool Contains(this LayerMask mask, int layer)
		{
			return mask == (mask | (1 << layer));
		}
	}
}