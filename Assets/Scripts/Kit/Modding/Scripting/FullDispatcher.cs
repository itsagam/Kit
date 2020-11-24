#if MODDING
using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
using XLua;
using Object = System.Object;

namespace Kit.Modding.Scripting
{
	/// <summary>A <see cref="SimpleDispatcher" /> with a deeper hooking with the mod.</summary>
	/// <seealso cref="Kit.Modding.Scripting.SimpleDispatcher" />
	public class FullDispatcher: SimpleDispatcher
	{
		protected LuaEnv scriptEnv;
		protected event Action updateEvent;
		protected event Action fixedUpdateEvent;
		protected event Action lateUpdateEvent;

		/// <summary>
		///     Calls <c>awake</c> on and hooks <c>start</c>, <c>update</c>, <c>lateUpdate</c> and <c>fixedUpdate</c> methods
		/// 	from the scripting environment.
		/// </summary>
		/// <param name="env">The scripting environment.</param>
		public void Hook(LuaEnv env)
		{
			scriptEnv = env;

			Action awakeAction = scriptEnv.Global.Get<Action>("awake");
			if (awakeAction != null)
				ExecuteSafe(awakeAction);

			CallStart().Forget();

			Action updateAction = scriptEnv.Global.Get<Action>("update");
			if (updateAction != null)
				updateEvent += () => ExecuteSafe(updateAction);

			Action lateUpdateAction = scriptEnv.Global.Get<Action>("lateUpdate");
			if (lateUpdateAction != null)
				lateUpdateEvent += () => ExecuteSafe(lateUpdateAction);

			Action fixedUpdateAction = scriptEnv.Global.Get<Action>("fixedUpdate");
			if (fixedUpdateAction != null)
				fixedUpdateEvent += () => ExecuteSafe(fixedUpdateAction);
		}

		protected async UniTaskVoid CallStart()
		{
			await UniTask.Yield(PlayerLoopTiming.Update);

			Action startAction = scriptEnv.Global.Get<Action>("start");
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

		/// <summary>Schedules an action on <c>update</c>, <c>lateUpdate</c> or <c>fixedUpdate</c> methods.</summary>
		/// <param name="type">The method to schedule on. Case-sensitive.</param>
		/// <param name="action">The piece of code to execute.</param>
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

		/// <summary>Stop all co-routines and calls <c>onDestroy</c> on mod scripts.</summary>
		public override void Stop()
		{
			if (scriptEnv == null)
				return;

			Action destroyAction = scriptEnv.Global.Get<Action>("onDestroy");
			if (destroyAction != null)
				ExecuteSafe(destroyAction);

			StopAllCoroutines();
			updateEvent = null;
			lateUpdateEvent = null;
			fixedUpdateEvent = null;
			scriptEnv = null;
		}
	}

	/*
	/// <summary>A <see cref="SimpleDispatcher" /> with a deeper hooking with the mod.</summary>
	/// <seealso cref="Kit.Modding.Scripting.SimpleDispatcher" />
	public class FullDispatcher: SimpleDispatcher
	{
		protected LuaEnv scriptEnv;
		protected CancellationTokenSource cancelSource = new CancellationTokenSource();

		/// <summary>
		///     Calls <c>awake</c> on and hooks <c>start</c>, <c>update</c>, <c>lateUpdate</c> and <c>fixedUpdate</c> methods
		/// 	from the scripting environment.
		/// </summary>
		/// <param name="env">The scripting environment.</param>
		public void Hook(LuaEnv env)
		{
			scriptEnv = env;

			Action awakeAction = scriptEnv.Global.Get<Action>("awake");
			if (awakeAction != null)
				ExecuteSafe(awakeAction);

			CallStart().Forget();

			Action updateAction = scriptEnv.Global.Get<Action>("update");
			if (updateAction != null)
				Schedule(PlayerLoopTiming.Update, updateAction);

			Action lateUpdateAction = scriptEnv.Global.Get<Action>("lateUpdate");
			if (lateUpdateAction != null)
				Schedule(PlayerLoopTiming.Update, lateUpdateAction);

			Action fixedUpdateAction = scriptEnv.Global.Get<Action>("fixedUpdate");
			if (fixedUpdateAction != null)
				Schedule(PlayerLoopTiming.Update, fixedUpdateAction);
		}

		protected async UniTaskVoid CallStart()
		{
			await UniTask.Yield();

			Action startAction = scriptEnv.Global.Get<Action>("start");
			if (startAction != null)
				ExecuteSafe(startAction);
		}

		/// <summary>Schedule an action on <c>update</c>, <c>lateUpdate</c> or <c>fixedUpdate</c> methods.</summary>
		/// <param name="type">The method to schedule on. Case-sensitive.</param>
		/// <param name="action">The piece of code to execute.</param>
		public void Schedule(string type, Action action)
		{
			if (action == null)
				return;

			switch (type)
			{
				case "update":
					Schedule(PlayerLoopTiming.Update, action);
					break;

				case "lateUpdate":
					Schedule(PlayerLoopTiming.PostLateUpdate, action);
					break;

				case "fixedUpdate":
					Schedule(PlayerLoopTiming.FixedUpdate, action);
					break;
			}
		}

		/// <summary>
		/// Schedule an action on a particular timing.
		/// </summary>
		/// <param name="timing"><see cref="PlayerLoopTiming"/> to schedule the action on.</param>
		/// <param name="action">The piece of code to execute.</param>
		public void Schedule(PlayerLoopTiming timing, Action action)
		{
			UniTaskAsyncEnumerable.EveryUpdate(timing)
								  .ForEachAsync(_ => ExecuteSafe(action), cancelSource.Token);
		}

		/// <summary>Stop all co-routines and calls <c>onDestroy</c> on mod scripts.</summary>
		public override void Stop()
		{
			if (scriptEnv == null)
				return;

			Action destroyAction = scriptEnv.Global.Get<Action>("onDestroy");
			if (destroyAction != null)
				ExecuteSafe(destroyAction);

			cancelSource.Cancel();
			cancelSource.Dispose();

			StopAllCoroutines();

			scriptEnv = null;
		}
	}
	*/
}
#endif