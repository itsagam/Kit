using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UniRx;
using System.Text;

public class Debugger : MonoBehaviour
{

	#region Profiling
	protected static OrderedDictionary Samples = new OrderedDictionary();

	public static void StartProfile(string name)
	{
		CustomSampler sample = CustomSampler.Create(name);
		sample.GetRecorder().enabled = true;
		sample.Begin();

		Samples.Add(name, sample);
	}

	public static CustomSampler GetProfile(string name)
	{
		if (Samples.Contains(name))
			return (CustomSampler)Samples[name];
		return null;
	}

	public static CustomSampler GetProfile(int index)
	{
		return (CustomSampler)Samples[index];
	}

	public static CustomSampler GetFirstProfile()
	{
		if (Samples.Count <= 0)
			return null;
		return GetProfile(0);
	}

	public static CustomSampler GetLastProfile()
	{
		if (Samples.Count <= 0)
			return null;
		return GetProfile(Samples.Count - 1);
	}

	public static void EndProfile()
	{
		GetLastProfile()?.End();
	}

	public static void EndProfile(string name)
	{
		GetProfile(name)?.End();
	}

	public static void EndAndLogProfile()
	{
		CustomSampler sample = GetLastProfile();
		if (sample != null)
		{
			sample.End();
			LogProfile(sample.name);
		}
	}

	public static void EndAndLogProfile(string name)
	{
		EndProfile(name);
		LogProfile(name);
	}

	public static void LogProfile(string name)
	{
		MainThreadDispatcher.StartUpdateMicroCoroutine(LogTimeLocal());
		IEnumerator LogTimeLocal()
		{
			yield return null;
			Debug.Log($"{name}: {GetTime(name)}");
		}
	}

	public static string GetTime(string name)
	{
		long time = GetTimeRaw(name);
		if (time < 0)
			return null;
		return Math.Round(time / 1000000000f, 3) + "s";
	}

	public static long GetTimeRaw(string name)
	{
		CustomSampler sample = GetProfile(name);
		if (sample != null)
			return sample.GetRecorder().elapsedNanoseconds;
		return -1;
	}
	#endregion

	#region Logging
	public static void Log(string line)
	{
		Debug.Log(line);
	}

	public static void Log(string type, string line)
	{
		Log("[" + type + "] " + line);
	}

	public static void Log(object obj)
	{
		Log(EncodeObject(obj));
	}

	public static void Log(string type, object obj)
	{
		Log(type, EncodeObject(obj.ToString()));
	}

	public static void Log(IEnumerable enumerable)
	{
		Log(EnumerableToString(enumerable));
	}

	public static void Log(string type, IEnumerable enumerable)
	{
		Log(type, EnumerableToString(enumerable));
	}

	protected static string EnumerableToString(IEnumerable enumerable)
	{
		IEnumerable<object> items = enumerable.Cast<object>();
		StringBuilder output = new StringBuilder();
		if (items.Any())
		{
			bool first = true;
			foreach (object obj in items)
				if (first)
				{
					output.Append(obj);
					first = false;
				}
				else
					output.Append(", " + obj);
		}
		return "{" + output + "}";
	}

	public static string EncodeObject(object data)
	{
		return JsonUtility.ToJson(data, true);
	}

	public static T DecodeObject<T>(string encoded)
	{
		return JsonUtility.FromJson<T>(encoded);
	}
	#endregion
}
