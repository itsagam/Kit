using UnityEngine;

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
				if (instance == null)
				{
					instance = (T)FindObjectOfType(typeof(T));
					if (instance == null)
					{
						GameObject singleton = new GameObject();
						instance = singleton.AddComponent<T>();
						singleton.name = typeof(T).ToString();
					}
				}
				return instance;
			}
		}
	}
}

public class SingletonPrefab<T> : MonoBehaviour where T : MonoBehaviour
{
	public const string Path = "Singletons";

	protected static T instance;
	protected static object mutex = new object();

	public static T Instance
	{
		get
		{
			lock (mutex)
			{
				if (instance == null)
				{
					instance = (T)FindObjectOfType(typeof(T));
					if (instance == null)
					{
						T prefab = Resources.Load<T>(Path + "/" + typeof(T).ToString());
						instance = GameObject.Instantiate<T>(prefab);
						instance.name = prefab.name;
					}
				}
				return instance;
			}
		}
	}
}