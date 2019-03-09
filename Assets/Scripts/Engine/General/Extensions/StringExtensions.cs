using System;
using System.Text.RegularExpressions;

public static class StringExtensions
{
	public static string Left(this string str, int count)
	{
		return count > str.Length ? str : str.Substring(0, count);
	}

	public static string Right(this string str, int count)
	{
		return count > str.Length ? str : str.Substring(str.Length - count);
	}

	public static string Slice(this string str, int startIndex)
	{
		return str.Substring(startIndex);
	}

	public static string Slice(this string str, int startIndex, int endIndex)
	{
		return str.Substring(startIndex, endIndex - startIndex);
	}

	public static bool IsLeft(this string str, string compare)
	{
		return str.Left(compare.Length) == compare;
	}

	public static bool IsRight(this string str, string compare)
	{
		return str.Right(compare.Length) == compare;
	}

	public static bool IsSlice(this string str, int startIndex, string compare)
	{
		return str.Slice(startIndex, compare.Length) == compare;
	}

	public static string[] SplitAndTrim(this string str, params char[] separators)
	{
		return Array.ConvertAll(str.Split(separators), p => p.Trim());
	}

	public static bool IsNullOrWhiteSpace(this string str)
	{
		return string.IsNullOrWhiteSpace(str);
	}

	public static bool IsNullOrEmpty(this string str)
	{
		return string.IsNullOrEmpty(str);
	}

	public static bool IsEmail(this string str)
	{
		const string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
							 + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
							 + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";
		Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
		return regex.IsMatch(str);
	}
}