using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EnumerableExtensions
{
	public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
	{
		foreach (T obj in enumerable)
			action(obj);
	}

	public static IEnumerable<T> Yield<T>(this T item)
	{
		yield return item;
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
}