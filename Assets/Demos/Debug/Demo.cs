using System;
using Kit;
using UnityEngine;
using Object = System.Object;

namespace Demos.Debug
{
	public class Demo: MonoBehaviour
	{
		public void LogTypes()
		{
			Debugger.Log("This is a Log message.");
			Debugger.Log("This is a Warning message.", LogType.Warning);
			Debugger.Log("This is an Assert message.", LogType.Assert);
			Debugger.Log("This is an Error message.",  LogType.Error);
			Debugger.Log("This is an Exception.",      LogType.Exception);
		}

		public void LogCategories()
		{
			Debugger.Log("Demo", "This is a Log message with Demo category.");
			Debugger.Log("Demo", "This is a Warning message with Demo category.", LogType.Warning);
			Debugger.Log("Demo", "This is an Assert message with Demo category.", LogType.Assert);
			Debugger.Log("Demo", "This is an Error message with Demo category.",  LogType.Error);
			Debugger.Log("Demo", "This is an Exception with Demo category.",      LogType.Exception);
		}

		public void LogCollection()
		{
			object[] array = new object[] { 1, 2, 3, new[] { "Red", "Green", "Blue" }, 4, 5 };

			// array.Log() will work just as well
			Debugger.Log(array);
		}

		private class User
		{
			public string FirstName, LastName;
			public int Age;
			public DateTime Joined;
		}
		public void LogObject()
		{
			User user = new User { FirstName = "Sultan", LastName = "Rahi", Age = 40, Joined = DateTime.Now};
			Debugger.Log(user, true);
		}

		public void Profile()
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			int times = 1000000;

			Canvas canvas = FindObjectOfType<Canvas>();

			Transform transform1 = GetComponent<Transform>();
			Transform transform2 = canvas.transform;

			Debugger.StartProfile("Uncached Components");
			for (int i = 0; i < times; i++)
			{
				float distance = (transform.position - canvas.transform.position).magnitude;
			}
			Debugger.EndProfile();

			Debugger.StartProfile("Cached Components");
			for (int i = 0; i < times; i++)
			{
				float distance = (transform1.position - transform2.position).magnitude;
			}
			Debugger.EndProfile();
#endif
		}
	}
}