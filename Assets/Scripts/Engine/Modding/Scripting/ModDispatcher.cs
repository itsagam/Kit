using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace Modding
{
	public class ModDispatcher : MonoBehaviour
	{
		protected static Transform parent = null;

		protected LuaEnv scriptEnv;
		protected event Action updateEvent;
		protected event Action fixedUpdateEvent;
		protected event Action lateUpdateEvent;

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

		public void Hook(LuaEnv env)
		{
			scriptEnv = env;

			var awakeAction = scriptEnv.Global.Get<Action>("awake");
			if (awakeAction != null)
				ExecuteSafe(awakeAction);

			var updateAction = scriptEnv.Global.Get<Action>("update");
			if (updateAction != null)
				updateEvent += () => ExecuteSafe(updateAction);

			var lateUpdateAction = scriptEnv.Global.Get<Action>("lateUpdate");
			if (lateUpdateAction != null)
				lateUpdateEvent += () => ExecuteSafe(lateUpdateAction);

			var fixedUpdateAction = scriptEnv.Global.Get<Action>("fixedUpdate");
			if (fixedUpdateAction != null)
				fixedUpdateEvent += () => ExecuteSafe(fixedUpdateAction);
		}

		protected void Start()
		{
			var startAction = scriptEnv.Global.Get<Action>("start");
			if (startAction != null)
				ExecuteSafe(startAction);
		}

		protected void Update()
		{
			updateEvent?.Invoke();
		}

		protected void LateUpdate()
		{
			lateUpdateEvent?.Invoke();
		}

		protected void FixedUpdate()
		{
			fixedUpdateEvent?.Invoke();
		}

		public void Schedule(string type, Action action)
		{
			if (action == null)
				return;

			switch (type)
			{
				case "update":
					updateEvent += () => ExecuteSafe(action);
					break;

				case "lateUpdate":
					lateUpdateEvent += () => ExecuteSafe(action);
					break;

				case "fixedUpdate":
					fixedUpdateEvent += () => ExecuteSafe(action);
					break;
			}
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

		public void Stop()
		{
			if (scriptEnv != null)
			{
				var destroyAction = scriptEnv.Global.Get<Action>("destroy");
				if (destroyAction != null)
					ExecuteSafe(destroyAction);

				StopAllCoroutines();
				updateEvent = null;
				lateUpdateEvent = null;
				fixedUpdateEvent = null;
				scriptEnv = null;
			}
		}
	}
}