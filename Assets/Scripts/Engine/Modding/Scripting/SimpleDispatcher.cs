using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Modding
{
	public class SimpleDispatcher : MonoBehaviour
	{
		protected static Transform parent = null;

		protected void CreateParent()
		{
			GameObject parentGO = new GameObject("Mods");
			GameObject.DontDestroyOnLoad(parentGO);
			parent = parentGO.transform;
		}

		protected void Awake()
		{
			if (parent == null)
				CreateParent();
			transform.parent = parent;
		}

		protected void ExecuteSafe(Action action)
		{
			try
			{
				action();
			}
			catch (Exception e)
			{
				Debugger.Log("ModManager", $"{name} – {e.Message}");
			}
		}

		protected IEnumerator ExecuteSafe(IEnumerator enumerator)
		{
			while (true)
			{
				try
				{
					if (enumerator.MoveNext() == false)
						yield break;
				}
				catch (Exception e)
				{
					Debugger.Log("ModManager", $"{name} – {e.Message}");
					yield break;
				}
				yield return enumerator.Current;
			}
		}

		public void StartCoroutineSafe(IEnumerator enumerator)
		{
			StartCoroutine(ExecuteSafe(enumerator));
		}

		protected void OnDestroy()
		{
			Stop();
		}

		public virtual void Stop()
		{
			StopAllCoroutines();
		}
	}
}