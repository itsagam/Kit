#if MODDING
using System;
using System.Collections;
using UnityEngine;

namespace Kit.Modding.Scripting
{
	/// <summary>A buddy <see cref="Component" />/<see cref="GameObject" /> for a mod.</summary>
	public class SimpleDispatcher: MonoBehaviour
	{
		/// <summary>Parent <see cref="Transform" /> for all mods.</summary>
		protected static Transform parent = null;

		/// <summary>Create the parent <see cref="GameObject" />/<see cref="Transform" /> for mods.</summary>
		protected static void CreateParent()
		{
			GameObject parentGO = new GameObject("Mods");
			DontDestroyOnLoad(parentGO);
			parent = parentGO.transform;
		}

		protected void Awake()
		{
			if (parent == null)
				CreateParent();
			transform.parent = parent;
		}

		/// <summary>Executes a piece of code such that exceptions are handled.</summary>
		protected void ExecuteSafe(Action action)
		{
			try
			{
				action();
			}
			catch (Exception e)
			{
				Debugger.Log(ModManager.LogCategory, $"{name} – {e.Message}");
			}
		}

		/// <summary>Executes a co-routine such that exceptions are handled.</summary>
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
					Debugger.Log(ModManager.LogCategory, $"{name} – {e.Message}");
					yield break;
				}

				yield return enumerator.Current;
			}
		}

		/// <summary>Starts a co-routine with exceptions handled.</summary>
		public void StartCoroutineSafe(IEnumerator enumerator)
		{
			StartCoroutine(ExecuteSafe(enumerator));
		}

		protected void OnDestroy()
		{
			Stop();
		}

		/// <summary>Stop all co-routines running with the component.</summary>
		public virtual void Stop()
		{
			StopAllCoroutines();
		}
	}
}
#endif