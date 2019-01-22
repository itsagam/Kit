using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class StringBuilderExtensions
{
	public static int IndexOf(this StringBuilder sb, char value, int startIndex)
	{
		for (int i = startIndex; i < sb.Length; i++)
			if (sb[i] == value)
				return i;
		return -1;
	}

	public static int IndexOf(this StringBuilder sb, string value, int startIndex)
	{
		int index;
		int length = value.Length;
		int maxSearchLength = (sb.Length - length) + 1;

		for (int i = startIndex; i < maxSearchLength; ++i)
		{
			if (sb[i] == value[0])
			{
				index = 1;
				while ((index < length) && (sb[i + index] == value[index]))
					++index;

				if (index == length)
					return i;
			}
		}

		return -1;
	}
}