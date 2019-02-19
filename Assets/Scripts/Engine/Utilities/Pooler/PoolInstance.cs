using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Component added to all instances so we can track the pool they came from
// Used in Pooler.Destroy to de-activate instances without providing pool
public class PoolInstance : MonoBehaviour
{
	public Pool Pool;
	public Component Component;

	// Instances should not destroy under normal circumstances, but are handled gracefully for fault-tolerance
	protected void OnDestroy()
	{
		// Find and remove individual instances from Pool only if the pool/scene itself is not being unloaded
		if (Pool != null && !Pool.IsDestroying)
		{
			if (!Pool.Used.Remove(Component))
				Pool.Available.Remove(Component);
		}
	}
}