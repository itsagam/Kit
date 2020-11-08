using Engine.Pooling;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Engine.Behaviours
{
	/// <summary>
	/// Pools the GameObject after a specified time.
	/// </summary>
	public class SelfPool: MonoBehaviour
	{
		/// <summary>
		/// The component to use for pooling.
		/// </summary>
		[Tooltip("The component to use as the key for pooling.")]
		public Component Component;

		/// <summary>
		/// Time to hold out for in seconds before pooling.
		/// </summary>
		[Tooltip("Time to hold out for before pooling.")]
		[SuffixLabel("seconds", true)]
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