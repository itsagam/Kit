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
	public const int LogProfileDelay = 1;
	public const string NullString = "Null";
	
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

	public static void EndAndLogProfile(int delayFrames = LogProfileDelay)
	{
		CustomSampler sample = GetLastProfile();
		if (sample != null)
		{
			sample.End();
			LogProfile(sample.name, delayFrames);
		}
	}

	public static void EndAndLogProfile(string name, int delayFrames = LogProfileDelay)
	{
		EndProfile(name);
		LogProfile(name, delayFrames);
	}

	public static void LogProfile(string name, int delayFrames = LogProfileDelay)
	{
		Log(name + ": " + GetTime(name));
		Observable.TimerFrame(delayFrames).Subscribe(t => Log(name + ": " + GetTime(name)));
	}

	public static string GetTime(string name)
	{
		long time = GetTimeRaw(name);
		if (time < 0)
			return null;
		return Math.Round(time / 1000000f, 5) + "ms";
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

	public static void Log(object obj, bool serialize = false)
	{
		Log(ObjectToString(obj, serialize));
	}

	public static void Log(string type, object obj, bool serialize = false)
	{
		Log(type, ObjectToString(obj, serialize));
	}

	public static void Log(IEnumerable enumerable, bool serialize = false)
	{
		Log(EnumerableToString(enumerable, serialize));
	}

	public static void Log(string type, IEnumerable enumerable, bool serialize = false)
	{
		Log(type, EnumerableToString(enumerable, serialize));
	}

	public static string ObjectOrEnumerableToString(object obj, bool serialize, string nullString = NullString)
	{
		if (obj is IEnumerable e && !(e is string))
			return EnumerableToString(e, serialize, nullString);
		else
			return ObjectToString(obj, serialize, nullString);
	}

	public static string ObjectToString(object obj, bool serialize, string nullString = NullString)
	{
		if (obj == null)
			return nullString;
		
		if (obj.GetType().IsValueType || (obj is string))
			return obj.ToString();
		else
			return serialize ? SerializeObject(obj) : obj.ToString();
	}

	public static string EnumerableToString(IEnumerable enumerable, bool serialize, string nullString = NullString)
	{
		if (enumerable == null)
			return nullString;

		StringBuilder output = new StringBuilder();
		output.Append("{");
		bool first = true;
		foreach (object item in enumerable)
		{
			string itemString = ObjectOrEnumerableToString(item, serialize, nullString);
			if (first)
				first = false;
			else
				output.Append(", ");
			output.Append(itemString);
		}
		output.Append("}");	
		return output.ToString();
	}

	public static string SerializeObject(object obj)
	{
		return JsonUtility.ToJson(obj, true);
	}
	#endregion
}
