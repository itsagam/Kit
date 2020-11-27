using Sirenix.OdinInspector;
using UnityEngine;

namespace Kit.Pooling
{
	/// <summary>Component added to all instances so we can handle its destruction gracefully.</summary>
	public class PoolInstance: MonoBehaviour
	{
		// Instances should not destroy under normal circumstances, but are handled gracefully for fault-tolerance
		protected void OnDestroy()
		{
			GameObject go = gameObject;
			PoolInstanceInfo info = Pooler.GetInstanceInfo(go);
			Pool pool = info.Pool;

			// Remove individual instances from Pool only if the pool/scene itself is not being unloaded
			if (pool != null && !pool.IsDestroying)
			{
				if (info.IsPooled)
					pool.Available.Remove(info.Component);
				else
					pool.Used.Remove(info.Component);
			}

			Pooler.InfoByGameObject.Remove(go);
		}

		/// <summary>Pool the instance.</summary>
		[PropertySpace]
		[Button(ButtonSizes.Large)]
		[DisableIf("IsPooled")]
		[LabelText("Move To Pool")]
		public void Pool()
		{
			Pooler.Destroy(this);
		}

		public bool IsPooled => Pooler.GetInstanceInfo(this).IsPooled;
	}
}