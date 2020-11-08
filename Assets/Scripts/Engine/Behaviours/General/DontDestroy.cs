using UnityEngine;

namespace Engine.Behaviours
{
	/// <summary>
	/// Marks the GameObject to be persistent across scenes.
	/// </summary>
	public class DontDestroy: MonoBehaviour
	{
		protected void Awake()
		{
			DontDestroyOnLoad(gameObject);
		}
	}
}