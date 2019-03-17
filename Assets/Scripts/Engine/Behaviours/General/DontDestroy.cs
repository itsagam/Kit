using UnityEngine;

namespace Engine.Behaviours
{
	public class DontDestroy : MonoBehaviour
	{
		protected void Awake()
		{
			DontDestroyOnLoad(gameObject);
		}
	}
}
