using UnityEngine;

namespace Kit
{
	/// <summary>
	/// <see cref="UnityEngine.Object"/> extensions.
	/// </summary>
	public static class UnityObjectExtensions
	{
		/// <summary>
		/// Destroy the object.
		/// </summary>
		public static void Destroy(this Object obj)
		{
			Object.Destroy(obj);
		}
	}
}