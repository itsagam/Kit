using System;
using System.Text.RegularExpressions;

namespace Engine
{
	/// <summary>
	/// <see cref="String"/> extensions.
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Returns a substring with the specified numbers of characters from the left side.
		/// </summary>
		public static string Left(this string str, int count)
		{
			return count > str.Length ? str : str.Substring(0, count);
		}

		/// <summary>
		/// Returns a substring with the specified numbers of characters from the right side.
		/// </summary>
		public static string Right(this string str, int count)
		{
			return count > str.Length ? str : str.Substring(str.Length - count);
		}

		/// <summary>
		/// Returns a substring starting at a specified index going till the end.
		/// </summary>
		public static string Slice(this string str, int startIndex)
		{
			return str.Substring(startIndex);
		}

		/// <summary>
		/// Returns a substring starting and ending at a specified index.
		/// </summary>
		public static string Slice(this string str, int startIndex, int endIndex)
		{
			return str.Substring(startIndex, endIndex - startIndex);
		}

		/// <summary>
		/// Returns whether the lift side of the string is the given one.
		/// </summary>
		public static bool IsLeft(this string str, string compare)
		{
			return str.Left(compare.Length) == compare;
		}

		/// <summary>
		/// Returns whether the right side of the string is the given one.
		/// </summary>
		public static bool IsRight(this string str, string compare)
		{
			return str.Right(compare.Length) == compare;
		}

		/// <summary>
		/// Returns whether a part of the string is equal to another string.
		/// </summary>
		public static bool IsSlice(this string str, int startIndex, string compare)
		{
			return str.Slice(startIndex, compare.Length) == compare;
		}

		/// <summary>
		/// Splits the strings by the specified delimiters and trims each one.
		/// </summary>
		public static string[] SplitAndTrim(this string str, params char[] separators)
		{
			return Array.ConvertAll(str.Split(separators), p => p.Trim());
		}

		/// <summary>
		/// Is the string <see langword="null"/> or white-space?
		/// </summary>
		public static bool IsNullOrWhiteSpace(this string str)
		{
			return string.IsNullOrWhiteSpace(str);
		}

		/// <summary>
		/// Is the string <see langword="null"/> or empty?
		/// </summary>
		public static bool IsNullOrEmpty(this string str)
		{
			return string.IsNullOrEmpty(str);
		}

		/// <summary>
		/// Does the string match the format of an email address?
		/// </summary>
		public static bool IsEmail(this string str)
		{
			return Regex.IsMatch(str,
								 @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z",
								 RegexOptions.IgnoreCase);
		}
	}
}