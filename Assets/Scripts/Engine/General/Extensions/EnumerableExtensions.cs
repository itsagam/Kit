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
}