using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Engine.Parsers;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

namespace Engine
{
	/// <summary>
	/// Debugging methods for logging and profiling.
	/// </summary>
	public static class Debugger
	{
		#region Profiling

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		private static Dictionary<string, CustomSampler> samples = new Dictionary<string, CustomSampler>();
		private static Stack<CustomSampler> runningSamples = new Stack<CustomSampler>();

		/// <summary>
		/// Start profiling a section of code.
		/// </summary>
		/// <param name="name">Name of the profile to be used for sampling.</param>
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

		/// <summary>
		/// Get the sampler of a profile.
		/// </summary>
		public static CustomSampler GetProfile(string name)
		{
			return samples.GetOrDefault(name);
		}

		/// <summary>
		/// Stop profiling the last section of code.
		/// </summary>
		public static void EndProfile()
		{
			runningSamples.Pop()?.End();
		}

		private static void LogProfile(Sampler sample)
		{
			Recorder recorder = sample.GetRecorder();
			recorder.enabled = true;
			Observable.EveryEndOfFrame()
					  .Subscribe(l =>
								 {
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
		/// <summary>
		/// The string to display for <see langword="null" /> objects.
		/// </summary>
		public const string NullString = "Null";

		/// <summary>
		/// Log a line.
		/// </summary>
		/// <param name="line">The line to log.</param>
		/// <param name="type">Type of log.</param>
		// Conditionals make calls to these methods not be compiled in Release builds
		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log(string line, LogType type = LogType.Log)
		{
			switch (type)
			{
				case LogType.Log:
					Debug.Log(line);
					break;

				case LogType.Warning:
					Debug.LogWarning(line);
					break;

				case LogType.Error:
					Debug.LogError(line);
					break;

				case LogType.Assert:
					Debug.LogAssertion(line);
					break;

				case LogType.Exception:
					Debug.LogException(new Exception(line));
					break;
			}
		}

		/// <summary>
		/// Log a line.
		/// </summary>
		/// <param name="category">Category of the log.</param>
		/// <param name="line">The line to log.</param>
		/// <param name="type">Type of log.</param>
		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log(string category, string line, LogType type = LogType.Log)
		{
			Log("[" + category + "] " + line, type);
		}

		/// <summary>
		/// Log an object.
		/// </summary>
		/// <param name="obj">The object to log. Can be a collection or a class.</param>
		/// <param name="serialize">Whether to serialize the object for display if it's a class.</param>
		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log(object obj, bool serialize = false)
		{
			Log(ObjectToString(obj, serialize));
		}

		/// <summary>
		/// Log an object.
		/// </summary>
		/// <param name="category">Category of the log.</param>
		/// <param name="obj">The object to log. Can be a collection or a class.</param>
		/// <param name="serialize">Whether to serialize the object for display if it's a class.</param>
		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log(string category, object obj, bool serialize = false)
		{
			Log(category, ObjectToString(obj, serialize));
		}

		/// <summary>
		/// Convert an object to a string for display.
		/// </summary>
		/// <param name="obj">The object to convert.</param>
		/// <param name="serialize">Whether to serialize the object if it's a class.</param>
		/// <param name="nullString">The string to use for null objects.</param>
		public static string ObjectToString(object obj, bool serialize, string nullString = NullString)
		{
			if (obj == null)
				return nullString;

			return serialize ? JSONParser.ToJson(obj) : obj.ToString();
		}

		/// <summary>
		/// Convert an object to a string and append it to a StringBuilder.
		/// </summary>
		/// <param name="output">The StringBuilder to append the result to.</param>
		/// <param name="obj">The object to convert.</param>
		/// <param name="serialize">Whether to serialize the object if it's a class.</param>
		/// <param name="nullString">The string to use for null objects.</param>
		public static void ObjectToString(StringBuilder output, object obj, bool serialize, string nullString)
		{
			output.Append(ObjectToString(obj, serialize, nullString));
		}
		#endregion
	}
}