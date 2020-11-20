using Kit.Pooling;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kit.Behaviours
{
	/// <summary>Pools the <see cref="GameObject" /> after a specified time.</summary>
	public class SelfPool: MonoBehaviour, IPooled
	{
		/// <summary>Time to hold out for in seconds before pooling.</summary>
		[Tooltip("Time to hold out for before pooling.")]
		[SuffixLabel("seconds", true)]
		public float Time = 5.0f;

		public void AwakeFromPool()
		{
			Invoke(nameof(Pool), Time);
		}

		public void OnDestroyIntoPool()
		{
			CancelInvoke(nameof(Pool));
		}

		public void Pool()
		{
			Pooler.Destroy(this);
		}
	}
}