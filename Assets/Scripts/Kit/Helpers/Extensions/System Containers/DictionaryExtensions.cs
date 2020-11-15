using System.Collections.Generic;

namespace Kit
{
	/// <summary>
	/// Dictionary extensions.
	/// </summary>
	public static class DictionaryExtensions
	{
		/// <summary>
		/// Allows to iterate over a dictionary using tuples.
		/// </summary>
		public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> tuple, out T1 key, out T2 value)
		{
			key = tuple.Key;
			value = tuple.Value;
		}

		/// <summary>
		/// Return the value of a given key if it exists or the default value (<see langword="null"/> for reference types) if not.
		/// </summary>
		public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
		{
			return dictionary.TryGetValue(key, out TValue value) ? value : default;
		}
	}
}