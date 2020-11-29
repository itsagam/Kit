using System;
using UnityEngine;

namespace Kit
{
	/// <summary>Helper functions for events and control flow.</summary>
	public class ControlHelper: MonoBehaviour
	{
		/// <summary>Event fired when the app loses/gains focus.</summary>
		public static event Action<bool> ApplicationFocus;

		/// <summary>Event fired when the app pauses/unpauses.</summary>
		public static event Action<bool> ApplicationPause;

		/// <summary>Event fired when the app quits.</summary>
		public static event Action ApplicationQuit;

		static ControlHelper()
		{
			GameObject go = new GameObject(nameof(ControlHelper));
			go.AddComponent<ControlHelper>();
			DontDestroyOnLoad(go);
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			ApplicationFocus?.Invoke(hasFocus);
		}

		private void OnApplicationPause(bool pauseStatus)
		{
			ApplicationPause?.Invoke(pauseStatus);
		}

		private void OnApplicationQuit()
		{
			ApplicationQuit?.Invoke();
		}
	}
}