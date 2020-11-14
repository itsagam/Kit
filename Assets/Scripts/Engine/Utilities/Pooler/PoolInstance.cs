using Sirenix.OdinInspector;
using UnityEngine;

namespace Engine.Pooling
{
	/// <summary>
	/// Component added to all instances so we can track the Pool they came from. Used in <see cref="Pooler.Destroy"/> to de-activate
	/// instances without providing pool.
	/// </summary>
	public class PoolInstance: MonoBehaviour
	{
		/// <summary>
		/// Pool this instance belong to.
		/// </summary>
		[Tooltip("Pool this instance belong to.")]
		public Pool Pool;

		/// <summary>
		/// The particular component being pooled.
		/// </summary>
		[Tooltip("The particular component being pooled.")]
		public Component Component;

		// Instances should not destroy under normal circumstances, but are handled gracefully for fault-tolerance
		protected void OnDestroy()
		{
			// Find and remove individual instances from Pool only if the pool/scene itself is not being unloaded
			if (Pool == null || Pool.IsDestroying)
				return;
			if (!Pool.Used.Remove(Component))
				Pool.Available.Remove(Component);
		}

		/// <summary>
		/// Pool the instance.
		/// </summary>
#if UNITY_EDITOR
		[PropertySpace]
		[Button(ButtonSizes.Large)]
		[ShowIf("IsValid")]
		[LabelText("Move To Pool")]
#endif
		public void Destroy()
		{
			Pool.Destroy(Component);
		}

		/// <summary>
		/// Is the instance properly configured?
		/// </summary>
		public bool IsValid => Pool != null && Component != null;
	}
}