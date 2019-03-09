#if MODDING
using System;
using XLua;

namespace Modding.Scripting
{
	public class FullDispatcher : SimpleDispatcher
	{
		protected LuaEnv scriptEnv;
		protected event Action updateEvent;
		protected event Action fixedUpdateEvent;
		protected event Action lateUpdateEvent;

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

		public override void Stop()
		{
			if (scriptEnv != null)
			{
				var destroyAction = scriptEnv.Global.Get<Action>("onDestroy");
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
#endif