using System;
using System.Collections.ObjectModel;

namespace Engine
{
	/// <summary>
	/// Array extensions.
	/// </summary>
	public static class ArrayExtensions
	{
		/// <summary>
		/// Find an item that matches a condition.
		/// </summary>
		public static T Find<T>(this T[] array, Predicate<T> predicate)
		{
			return Array.Find(array, predicate);
		}

		/// <summary>
		/// Find an item that matches a condition.
		/// </summary>
		public static T FindLast<T>(this T[] array, Predicate<T> predicate)
		{
			return Array.FindLast(array, predicate);
		}

		/// <summary>
		/// Find an item that matches a condition.
		/// </summary>
		public static int FindIndex<T>(this T[] array, Predicate<T> predicate)
		{
			return Array.FindIndex(array, predicate);
		}

		/// <summary>
		/// Find the index of an item that matches a condition.
		/// </summary>
		public static int FindIndex<T>(this T[] array, int startIndex, Predicate<T> predicate)
		{
			return Array.FindIndex(array, startIndex, predicate);
		}

		/// <summary>
		/// Find the index of an item that matches a condition.
		/// </summary>
		public static int FindIndex<T>(this T[] array, int startIndex, int count, Predicate<T> predicate)
		{
			return Array.FindIndex(array, startIndex, count, predicate);
		}

		/// <summary>
		/// Find the index of the last item that matches a condition.
		/// </summary>
		public static int FindLastIndex<T>(this T[] array, Predicate<T> predicate)
		{
			return Array.FindLastIndex(array, predicate);
		}

		/// <summary>
		/// Find the index of the last item that matches a condition.
		/// </summary>
		public static int FindLastIndex<T>(this T[] array, int startIndex, Predicate<T> predicate)
		{
			return Array.FindLastIndex(array, startIndex, predicate);
		}

		/// <summary>
		/// Find the index of the last item that matches a condition.
		/// </summary>
		public static int FindLastIndex<T>(this T[] array, int startIndex, int count, Predicate<T> predicate)
		{
			return Array.FindLastIndex(array, startIndex, count, predicate);
		}

		/// <summary>
		/// Find all items that match a condition.
		/// </summary>
		public static T[] FindAll<T>(this T[] array, Predicate<T> predicate)
		{
			return Array.FindAll(array, predicate);
		}

		/// <summary>
		/// Find the index of an item.
		/// </summary>
		public static int IndexOf<T>(this T[] array, T item)
		{
			return Array.IndexOf(array, item);
		}

		/// <summary>
		/// Find the index of an item.
		/// </summary>
		public static int IndexOf<T>(this T[] array, T item, int startIndex)
		{
			return Array.IndexOf(array, item, startIndex);
		}

		/// <summary>
		/// Find the index of an item.
		/// </summary>
		public static int IndexOf<T>(this T[] array, T item, int startIndex, int count)
		{
			return Array.IndexOf(array, item, startIndex, count);
		}

		/// <summary>
		/// Find the last index of an item.
		/// </summary>
		public static int LastIndexOf<T>(this T[] array, T item)
		{
			return Array.LastIndexOf(array, item);
		}

		/// <summary>
		/// Find the last index of an item.
		/// </summary>
		public static int LastIndexOf<T>(this T[] array, T item, int startIndex)
		{
			return Array.LastIndexOf(array, item, startIndex);
		}

		/// <summary>
		/// Find the last index of an item.
		/// </summary>
		public static int LastIndexOf<T>(this T[] array, T item, int startIndex, int count)
		{
			return Array.LastIndexOf(array, item, startIndex, count);
		}

		/// <summary>
		/// Clear the array.
		/// </summary>
		public static void Clear<T>(this T[] array)
		{
			Array.Clear(array, 0, array.Length);
		}

		/// <summary>
		/// Clear a portion of the array.
		/// </summary>
		public static void Clear<T>(this T[] array, int startIndex, int length)
		{
			Array.Clear(array, startIndex, length);
		}

		/// <summary>
		/// Return the array as a read-only collection.
		/// </summary>
		public static ReadOnlyCollection<T> AsReadOnly<T>(this T[] array)
		{
			return Array.AsReadOnly(array);
		}
	}
}