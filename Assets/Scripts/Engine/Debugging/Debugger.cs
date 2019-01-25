using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UniRx;
using Newtonsoft.Json;

public class Debugger : MonoBehaviour
{
	public const string NullString = "Null";

#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
		// Recorder values are valid only for one frame, and in which frame they register Begin/End seems random
		// This checks for registered recorder blocks for 100 frames after a call to this function
		IDisposable observer = null;
		int count = 0;	
		observer = Observable.EveryUpdate().Subscribe(l => {
			Recorder recorder = GetProfile(name)?.GetRecorder();
			if (recorder != null && recorder.sampleBlockCount > 0)
			{
				Log(name + ": " + ConvertTime(recorder.elapsedNanoseconds));
				observer.Dispose();
			}
			else if (count++ >= 100)
			{
				observer.Dispose();
			}
		});
	}

	public static string ConvertTime(long time)
	{
		if (time < 0)
			return null;
		return Math.Round(time / 1000000f, 5) + "ms";
	}
	#endregion
#endif

	#region Logging
	public static void Log(string line)
	{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		Debug.Log(line);
#endif
	}

	public static void Log(string type, string line)
	{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		Log("[" + type + "] " + line);
#endif
	}

	public static void Log(object obj, bool serialize = false)
	{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		Log(ObjectToString(obj, serialize));
#endif
	}

	public static void Log(string type, object obj, bool serialize = false)
	{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		Log(type, ObjectToString(obj, serialize));
#endif
	}

	public static void Log(IEnumerable enumerable, bool serialize = false)
	{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		Log(EnumerableToString(enumerable, serialize));
#endif
	}

	public static void Log(string type, IEnumerable enumerable, bool serialize = false)
	{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		Log(type, EnumerableToString(enumerable, serialize));
#endif
	}

	public static string ObjectOrEnumerableToString(object obj, bool serialize, string nullString = NullString)
	{
		var output = new StringBuilder();
		ObjectOrEnumerableToString(output, obj, serialize, nullString);
		return output.ToString();
	}

	public static void ObjectOrEnumerableToString(StringBuilder output, object obj, bool serialize, string nullString)
	{
		if (obj is IEnumerable enumerable && !(enumerable is string))
			EnumerableToString(output, enumerable, serialize, nullString);
		else
			ObjectToString(output, obj, serialize, nullString);
	}

	public static string ObjectToString(object obj, bool serialize, string nullString = NullString)
	{
		if (obj == null)
			return nullString;

		if (obj.GetType().IsValueType || (obj is string))
			return obj.ToString();
		else
			return serialize ? JsonConvert.SerializeObject(obj) : obj.ToString();
	}

	public static void ObjectToString(StringBuilder output, object obj, bool serialize, string nullString)
	{
		output.Append(ObjectToString(obj, serialize, nullString));
	}

	public static string EnumerableToString(IEnumerable enumerable, bool serialize, string nullString = NullString)
	{
		var output = new StringBuilder();
		EnumerableToString(output, enumerable, serialize, nullString);
		return output.ToString();
	}

	public static void EnumerableToString(StringBuilder output, IEnumerable enumerable, bool serialize, string nullString)
	{
		if (enumerable == null)
		{
			output.Append(nullString);
			return;
		}

		output.Append("{");
		bool first = true;
		foreach (object item in enumerable)
		{
			if (first)
				first = false;
			else
				output.Append(", ");
			ObjectOrEnumerableToString(output, item, serialize, nullString);
		}
		output.Append("}");
	}
	#endregion
}
