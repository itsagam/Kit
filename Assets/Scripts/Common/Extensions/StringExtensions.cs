using System;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public static class StringExtensions
{
	public static string Slice(this string str, int startIndex, int endIndex)
	{
		return str.Substring(startIndex, endIndex - startIndex);
	}

	public static string[] SplitAndTrim(this string str, params char[] separators)
	{
		return Array.ConvertAll(str.Split(separators), p => p.Trim());
	}

	public static bool IsNullOrEmpty(this string str)
	{
		return string.IsNullOrEmpty(str);
	}

	public static bool IsEmail(this string str)
	{
		string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|" 
			+ @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)" 
			+ @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";
		Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
		return regex.IsMatch(str);
	}
}