using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ControlHelper : Singleton<ControlHelper>
{
	protected void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}

	public static void DelayedCall(float time, Action action)
	{
		DelayedCall(new WaitForSeconds(time), action);
	}

	public static void DelayedCall(Func<bool> until, Action action)
	{
		DelayedCall(new WaitUntil(until), action);
	}

	public static void DelayedCall(Action action)
	{
		DelayedCall(new WaitForEndOfFrame(), action);
	}
	
	public static void DelayedCall(YieldInstruction instruction, Action action)
	{
		Instance.StartCoroutine(DelayedCallLocal());
		IEnumerator DelayedCallLocal()
		{
			yield return instruction;
			if (action != null)
				action();
		}
	}

	public static void DelayedCall(IEnumerator enumerator, Action action)
	{
		Instance.StartCoroutine(DelayedCallLocal());
		IEnumerator DelayedCallLocal()
		{
			yield return enumerator;
			if (action != null)
				action();
		}
	}

	public static Coroutine RunCoroutine(IEnumerator enumerator)
	{
		return Instance.StartCoroutine(enumerator);
	}

	public static CoroutineWithData RunCoroutineWithData(IEnumerator coroutine)
	{
		return new CoroutineWithData(Instance, coroutine);
	}
}

public class CoroutineWithData
{
	public Coroutine coroutine { get; private set; }
	public object result;
	private IEnumerator target;

	public CoroutineWithData(MonoBehaviour owner, IEnumerator target)
	{
		this.target = target;
		this.coroutine = owner.StartCoroutine(Run());
	}

	private IEnumerator Run()
	{
		while (target.MoveNext())
		{
			result = target.Current;
			yield return result;
		}
	}
}