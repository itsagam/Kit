using UnityEngine;

namespace Kit.Pooling
{
	/// <summary>Information cached about a pool instance.</summary>
	public class PoolInstanceInfo
	{
		/// <summary>The pool it belongs to.</summary>
		public Pool Pool;

		/// <summary>The component returned when initializing.</summary>
		public Component Component;

		/// <summary>Whether the instance is being used or not.</summary>
		public bool IsPooled = true;

		public PoolInstanceInfo(Pool pool, Component component)
		{
			Component = component;
			Pool = pool;
		}
	}
}