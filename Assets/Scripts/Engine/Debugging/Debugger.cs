using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Engine.Parsers;
using UniRx;
using UnityEngine;
using UnityEngine.Profiling;

namespace Engine
{
	public static class Debugger
	{
		public const string NullString = "Null";

		#region Profiling
	#if UNITY_EDITOR || DEVELOPMENT_BUILD
		private static Dictionary<string, CustomSampler> samples = new Dictionary<string, CustomSampler>();
		private static Stack<CustomSampler> runningSamples = new Stack<CustomSampler>();

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
			return samples.TryGetValue(name, out CustomSampler sample) ? sample : null;
		}

		public static void EndProfile()
		{
			runningSamples.Pop()?.End();
		}

		private static void LogProfile(CustomSampler sample)
		{
			Recorder recorder = sample.GetRecorder();
			recorder.enabled = true;
			Observable.EveryEndOfFrame().Subscribe(l => {
				if (recorder.sampleBlockCount > 0)
					Log(sample.name + ": " + ConvertTime(recorder.elapsedNanoseconds));
			});
		}

		private static string ConvertTime(long time)
		{
			if (time < 0)
				return null;
			return Math.Round(time / 1000000f, 5) + "ms";
		}
	#endif
		#endregion

		#region Logging
		// Conditionals make these calls to these methods be compiled in Release builds
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
		public static void Log(object obj, bool serialize = false)
		{
			Log(ObjectToString(obj, serialize));
		}

		[Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
		public static void Log(string category, object obj, bool serialize = false)
		{
			Log(category, ObjectToString(obj, serialize));
		}

		public static string ObjectToString(object obj, bool serialize, string nullString = NullString)
		{
			if (obj == null)
				return nullString;

			return serialize ? JSONParser.ToJson(obj) : obj.ToString();
		}

		public static void ObjectToString(StringBuilder output, object obj, bool serialize, string nullString)
		{
			output.Append(ObjectToString(obj, serialize, nullString));
		}
		#endregion
	}
}