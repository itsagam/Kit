using Engine.Pooling;
using UnityEngine;

namespace Engine.Behaviours
{
	public class SelfPool: MonoBehaviour
	{
		public Component Component;
		public float Time = 5.0f;

		protected void Start()
		{
			Invoke(nameof(Pool), Time);
		}

		protected void Pool()
		{
			Pooler.Destroy(Component);
		}
	}
}