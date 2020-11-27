using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Kit.Parsers;
using UniRx;
using UnityEngine;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

namespace Kit
{
	/// <summary>Debugging methods for logging and profiling.</summary>
	/// <remarks>
	///     The <see cref="Debugger.Log(object, bool)" /> method is particularly useful for displaying the contents of any object or
	///     collection.
	/// </remarks>
	public static class Debugger
	{
		private static readonly Dictionary<LogType, string> LogColors = new Dictionary<LogType, string>
																		{
																			{ LogType.Log, "7DE17D" },
																			{ LogType.Warning, "FFB247" },
																			{ LogType.Assert, "60A9FF" },
																			{ LogType.Error, "FF746F" },
																			{ LogType.Exception, "FF746F" }
																		};

		#region Profiling

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		private static Dictionary<string, CustomSampler> samples = new Dictionary<string, CustomSampler>();
		private static Stack<CustomSampler> runningSamples = new Stack<CustomSampler>();

		/// <summary>Start profiling a section of code.</summary>
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

		/// <summary>Get the sampler of a profile.</summary>
		public static CustomSampler GetProfile(string name)
		{
			return samples.GetOrDefault(name);
		}

		/// <summary>Stop profiling the last section of code.</summary>
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

		/// <summary>The string to display for <see langword="null" /> objects.</summary>
		public const string NullString = "Null";

		// Conditionals make calls to these methods not be compiled in Release builds.

		/// <summary>Log an exception.</summary>
		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log(Exception ex)
		{
			Debug.LogException(ex);
		}

		/// <summary>Log a line.</summary>
		/// <param name="line">The line to log.</param>
		/// <param name="type">Type of log.</param>
		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log(string line, LogType type = LogType.Log)
		{
			LogInternal($"<color=#{LogColors[type]}>{line}</color>", type);
		}

		/// <summary>Log a line.</summary>
		/// <param name="category">Category of the log.</param>
		/// <param name="line">The line to log.</param>
		/// <param name="type">Type of log.</param>
		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log(string category, string line, LogType type = LogType.Log)
		{
			LogInternal($"<color=#{LogColors[type]}><b>{category} ►</b> {line}</color>", type);
		}

		private static void LogInternal(string line, LogType type)
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

		/// <summary>Log an object.</summary>
		/// <param name="obj">The object to log. Can be a collection or a class.</param>
		/// <param name="serialize">Whether to serialize objects for display.</param>
		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log(object obj, bool serialize = false)
		{
			Log(ObjectOrEnumerableToString(obj, serialize));
		}

		/// <inheritdoc cref="Log(object, bool)" />
		/// <param name="category">Category of the log.</param>
		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log(string category, object obj, bool serialize = false)
		{
			Log(category, ObjectOrEnumerableToString(obj, serialize));
		}

		/// <summary>Log a collection.</summary>
		/// <param name="enumerable">The collection to log.</param>
		/// <param name="serialize">Whether to serialize the objects for display.</param>
		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log(IEnumerable enumerable, bool serialize = false)
		{
			Log(EnumerableToString(enumerable, serialize));
		}

		/// <inheritdoc cref="Log(IEnumerable, bool)" />
		/// <param name="category">Category of the log.</param>
		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log(string category, IEnumerable enumerable, bool serialize = false)
		{
			Log(category, EnumerableToString(enumerable, serialize));
		}

		/// <summary>Convert an object or collection to a string for display.</summary>
		/// <param name="obj">The object to convert.</param>
		/// <param name="serialize">Whether to serialize the object if it's a class.</param>
		/// <param name="nullString">The string to use for <see langword="null" /> objects.</param>
		public static string ObjectOrEnumerableToString(object obj, bool serialize, string nullString = NullString)
		{
			StringBuilder output = new StringBuilder();
			ObjectOrEnumerableToString(output, obj, serialize, nullString);
			return output.ToString();
		}

		/// <inheritdoc cref="ObjectOrEnumerableToString(object, bool, string)" />
		/// <param name="output"><see cref="StringBuilder" /> to append the result to.</param>
		public static void ObjectOrEnumerableToString(StringBuilder output, object obj, bool serialize, string nullString)
		{
			if (obj is IEnumerable enumerable && !(enumerable is string))
				EnumerableToString(output, enumerable, serialize, nullString);
			else
				ObjectToString(output, obj, serialize, nullString);
		}

		private static string EnumerableToString(IEnumerable enumerable, bool serialize, string nullString = NullString)
		{
			StringBuilder output = new StringBuilder();
			EnumerableToString(output, enumerable, serialize, nullString);
			return output.ToString();
		}

		private static void EnumerableToString(StringBuilder output, IEnumerable enumerable, bool serialize, string nullString = NullString)
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

		private static string ObjectToString(object obj, bool serialize, string nullString = NullString)
		{
			if (obj == null)
				return nullString;

			return serialize ? JsonParser.ToJson(obj) : obj.ToString();
		}

		private static void ObjectToString(StringBuilder output, object obj, bool serialize, string nullString = NullString)
		{
			output.Append(ObjectToString(obj, serialize, nullString));
		}

		#endregion
	}
}