using UnityEngine;

namespace Engine
{
	/// <summary>
	/// MonoBehaviours that create just one instance can inherit from this class for global access.
	/// </summary>
	/// <remarks>Should be used very sparingly.</remarks>
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