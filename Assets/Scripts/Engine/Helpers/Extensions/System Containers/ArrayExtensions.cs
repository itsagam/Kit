using System;
using System.Collections.ObjectModel;

namespace Engine
{
	public static class ArrayExtensions
	{
		public static T Find<T>(this T[] array, Predicate<T> predicate)
		{
			return Array.Find(array, predicate);
		}

		public static T FindLast<T>(this T[] array, Predicate<T> predicate)
		{
			return Array.FindLast(array, predicate);
		}

		public static int FindIndex<T>(this T[] array, Predicate<T> predicate)
		{
			return Array.FindIndex(array, predicate);
		}

		public static int FindIndex<T>(this T[] array, int startIndex, Predicate<T> predicate)
		{
			return Array.FindIndex(array, startIndex, predicate);
		}

		public static int FindIndex<T>(this T[] array, int startIndex, int count, Predicate<T> predicate)
		{
			return Array.FindIndex(array, startIndex, count, predicate);
		}

		public static int FindLastIndex<T>(this T[] array, Predicate<T> predicate)
		{
			return Array.FindLastIndex(array, predicate);
		}

		public static int FindLastIndex<T>(this T[] array, int startIndex, Predicate<T> predicate)
		{
			return Array.FindLastIndex(array, startIndex, predicate);
		}

		public static int FindLastIndex<T>(this T[] array, int startIndex, int count, Predicate<T> predicate)
		{
			return Array.FindLastIndex(array, startIndex, count, predicate);
		}

		public static T[] FindAll<T>(this T[] array, Predicate<T> predicate)
		{
			return Array.FindAll(array, predicate);
		}

		public static int IndexOf<T>(this T[] array, T item)
		{
			return Array.IndexOf(array, item);
		}

		public static int IndexOf<T>(this T[] array, T item, int startIndex)
		{
			return Array.IndexOf(array, item, startIndex);
		}

		public static int IndexOf<T>(this T[] array, T item, int startIndex, int count)
		{
			return Array.IndexOf(array, item, startIndex, count);
		}

		public static int LastIndexOf<T>(this T[] array, T item)
		{
			return Array.LastIndexOf(array, item);
		}

		public static int LastIndexOf<T>(this T[] array, T item, int startIndex)
		{
			return Array.LastIndexOf(array, item, startIndex);
		}

		public static int LastIndexOf<T>(this T[] array, T item, int startIndex, int count)
		{
			return Array.LastIndexOf(array, item, startIndex, count);
		}

		public static void Clear<T>(this T[] array, int index, int length)
		{
			Array.Clear(array, index, length);
		}

		public static ReadOnlyCollection<T> AsReadOnly<T>(this T[] array)
		{
			return Array.AsReadOnly(array);
		}
	}
}