using UnityEngine;

namespace Engine.Behaviours
{
	public class SelfDestroy: MonoBehaviour
	{
		public float Time = 5.0f;

		protected void Start()
		{
			Destroy(gameObject, Time);
		}
	}
}