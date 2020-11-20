using System.Collections.Generic;

namespace Kit
{
	public static class ListExtensions
	{
		/// <summary>Returns a random element from a list or array.</summary>
		/// <returns>A random element, or <see langword="null"/> if the list is empty.</returns>
		public static T GetRandom<T>(this IReadOnlyList<T> list) where T:class
		{
			return list.Count > 0 ? list[UnityEngine.Random.Range(0, list.Count)] : null;
		}
	}
}