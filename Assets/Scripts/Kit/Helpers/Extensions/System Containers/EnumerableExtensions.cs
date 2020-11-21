using System;
using System.Collections.Generic;
using System.Text;

namespace Kit
{
	/// <summary>LINQ extensions.</summary>
	public static class EnumerableExtensions
	{
		/// <summary>Return just the one item specified.</summary>
		public static IEnumerable<T> One<T>(T item)
		{
			yield return item;
		}

		/// <summary>Perform an action on a enumerable of items.</summary>
		public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
		{
			foreach (T obj in enumerable)
				action(obj);
		}

		/// <summary>Log all the items in enumerable to the Console.</summary>
		public static void Log<T>(this IEnumerable<T> enumerable, bool serialize = false)
		{
			Debugger.Log(enumerable, serialize);
		}

		/// <summary>Return the index of an item, or -1 if not found.</summary>
		public static int IndexOf<T>(this IEnumerable<T> source, T value)
		{
			int index = 0;
			foreach (T item in source)
			{
				if (item.Equals(value))
					return index;
				index++;
			}

			return -1;
		}

		/// <summary>Return the index of an item, or -1 if not found.</summary>
		public static int IndexOf<T>(this IEnumerable<T> source, T value, IEqualityComparer<T> comparer)
		{
			int index = 0;
			foreach (T item in source)
			{
				if (comparer.Equals(item, value))
					return index;
				index++;
			}

			return -1;
		}

		/// <summary>Combine a enumerable of strings separated by a delimiter.</summary>
		public static string Join(this IEnumerable<string> source, string separator = ", ")
		{
			return string.Join(separator, source);
		}

		/// <summary>Combine a enumerable of strings separated by a delimiter and append them to a <see cref="StringBuilder" />.</summary>
		public static void Join(this IEnumerable<string> source, StringBuilder builder, string separator = ", ")
		{
			bool first = true;
			foreach (string str in source)
			{
				if (first)
					first = false;
				else
					builder.Append(separator);
				builder.Append(str);
			}
		}
	}
}