using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UniRx;

public class Debugger : MonoBehaviour
{
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
}
