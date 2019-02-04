using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UniRx;
using Newtonsoft.Json;

public class Debugger
{
	public const string NullString = "Null";

	#region Profiling
#if UNITY_EDITOR || DEVELOPMENT_BUILD
	protected static Dictionary<string, CustomSampler> samples = new Dictionary<string, CustomSampler>();
	protected static Stack<CustomSampler> runningSamples = new Stack<CustomSampler>();

	public static void StartProfile(string name)
	{
		CustomSampler sample = GetProfile(name);
		if (sample == null)
		{
			sample = CustomSampler.Create(name);
			samples.Add(name, sample);
			LogProfile(sample);
		}
		runningSamples.Push(sample);
		sample.Begin();
	}

	public static CustomSampler GetProfile(string name)
	{
		if (samples.TryGetValue(name, out CustomSampler sample))
			return sample;
		return null;
	}

	public static void EndProfile()
	{
		runningSamples.Pop()?.End();
	}

	protected static void LogProfile(CustomSampler sample)
	{
		Recorder recorder = sample.GetRecorder();
		recorder.enabled = true;
		Observable.EveryEndOfFrame().Subscribe(l => {
			if (recorder.sampleBlockCount > 0)
				Log(sample.name + ": " + ConvertTime(recorder.elapsedNanoseconds));
		});
	}

	protected static string ConvertTime(long time)
	{
		if (time < 0)
			return null;
		return Math.Round(time / 1000000f, 5) + "ms";
	}
#endif
	#endregion

	#region Logging

	// Conditionals make these methods calls be ignored in Release builds
	[Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
	public static void Log(string line, LogType type = LogType.Log)
	{
		switch (type)
		{
			case LogType.Log:
				UnityEngine.Debug.Log(line);
				break;

			case LogType.Warning:
				UnityEngine.Debug.LogWarning(line);
				break;

			case LogType.Error:
				UnityEngine.Debug.LogError(line);
				break;

			case LogType.Assert:
				UnityEngine.Debug.LogAssertion(line);
				break;

			case LogType.Exception:
				UnityEngine.Debug.LogException(new Exception(line));
				break;
		}
	}

	[Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
	public static void Log(string category, string line, LogType type = LogType.Log)
	{
		Log("[" + category + "] " + line, type);
	}

	[Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
	public static void Log(object obj, bool serialize = true)
	{
		Log(ObjectToString(obj, serialize));
	}

	[Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
	public static void Log(string category, object obj, bool serialize = true)
	{
		Log(category, ObjectToString(obj, serialize));
	}

	public static string ObjectToString(object obj, bool serialize, string nullString = NullString)
	{
		if (obj == null)
			return nullString;

		if (serialize)
			return JsonConvert.SerializeObject(obj, Formatting.Indented);
		else
			return obj.ToString();
	}

	public static void ObjectToString(StringBuilder output, object obj, bool serialize, string nullString)
	{
		output.Append(ObjectToString(obj, serialize, nullString));
	}

	/*
	[Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
	public static void Log(IEnumerable enumerable, bool serialize = false)
	{
		Log(EnumerableToString(enumerable, serialize));
	}
	
	[Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
	public static void Log(string type, IEnumerable enumerable, bool serialize = false)
	{
		Log(type, EnumerableToString(enumerable, serialize));
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
	*/

	#endregion
}
