using System;
using System.Collections.Generic;
using System.Text;

namespace Engine
{
	public static class EnumerableExtensions
	{
		public static IEnumerable<T> One<T>(T item)
		{
			yield return item;
		}

		public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
		{
			foreach (T obj in enumerable)
				action(obj);
		}

		public static void Log<T>(this IEnumerable<T> enumerable, bool serialize = false)
		{
			enumerable.ForEach(o => Debugger.Log(o, serialize));
		}

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

		public static string Join(this IEnumerable<string> source, string separator = ", ")
		{
			return string.Join(separator, source);
		}

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