using Sirenix.OdinInspector;
using UnityEngine;

namespace Kit.Behaviours
{
	/// <summary>Marks the <see cref="GameObject" /> for destruction after a specified time.</summary>
	public class DestroyWithTime: MonoBehaviour
	{
		/// <summary>Time to hold out for in seconds before destroying.</summary>
		[Tooltip("The time to hold out for before destroying.")]
		[SuffixLabel("seconds", true)]
		public float Time = 5.0f;

		protected virtual void Start()
		{
			Destroy(gameObject, Time);
		}
	}
}