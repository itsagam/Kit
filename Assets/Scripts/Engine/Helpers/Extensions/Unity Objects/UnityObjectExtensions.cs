using UnityEngine;

namespace Engine
{
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