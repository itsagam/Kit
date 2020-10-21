using UnityEngine;

namespace Engine
{
	public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		protected static T instance;
		protected static object mutex = new object();

		public static T Instance
		{
			get
			{
				lock (mutex)
				{
					if (instance != null)
						return instance;

					instance = FindObjectOfType<T>();
					if (instance != null)
						return instance;

					GameObject gameObject = new GameObject();
					instance = gameObject.AddComponent<T>();
					gameObject.name = nameof(T);
					return instance;
				}
			}
		}
	}
}