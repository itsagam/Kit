using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kit.Pooling
{
	/// <summary>
	///     A PoolGroup represents a collection of <see cref="Pool" />s grouped by name. Can be used to group together pools of
	///     different types and components, or configure them together.
	/// </summary>
	[AddComponentMenu("Pooling/PoolGroup")]
	public class PoolGroup: MonoBehaviour, IEnumerable<Pool>
	{
		#region Fields

		/// <summary><see cref="Pool.MessageMode" /> of pools added to this group.</summary>
		[LabelText("Message")]
		[Tooltip("MessageMode of pools added to this group.")]
		[OnValueChanged("ResetMessageMode")]
		public PoolMessageMode MessageMode = PoolMessageMode.None;

		/// <summary>Whether to organize groups, pools, and instances for a cleaner scene hierarchy?</summary>
		[Tooltip("Whether to organize groups, pools, and instances for a cleaner scene hierarchy?")]
		[OnValueChanged("ResetOrganize")]
		public bool Organize = true;

		/// <summary>Keep pools under this group persistent across scenes.</summary>
		[Tooltip("Keep pools under this group persistent across scenes.")]
		[ShowIf("ShowPersistent")]
		public bool Persistent = false;

		/// <summary>List of pools associated with this group.</summary>
		[PropertySpace]
		[SceneObjectsOnly]
		[InlineEditor]
		[ListDrawerSettings(CustomAddFunction = "AddPool_Editor", CustomRemoveIndexFunction = "DestroyPool_Editor")]
		[Tooltip("List of pools associated with this group.")]
		public List<Pool> Pools = new List<Pool>();

		public bool IsDestroying { get; protected set; }

		#endregion

		#region Initialization

		protected void Awake()
		{
			Pooler.PoolGroupsByName.Add(name, this);
		}

		protected void Start()
		{
			if (Persistent && transform.parent == null)
				DontDestroyOnLoad(gameObject);
		}

		protected void OnDestroy()
		{
			IsDestroying = true;
			Pooler.PoolGroupsByName.Remove(name);
		}

		#endregion

		#region Pool management

		/// <summary>Add a pool to this group.</summary>
		public void AddPool(Pool pool)
		{
			Pools.Add(pool);
			pool.transform.parent = transform;

			// pool.Group = this;
			pool.MessageMode = MessageMode;
			pool.Organize = Organize;
			pool.Persistent = false;
		}

		/// <summary>Return whether a pool is a part of this group.</summary>
		public bool ContainsPool(Pool pool)
		{
			return Pools.Contains(pool);
		}

		/// <summary>Remove a pool from this group.</summary>
		/// <returns>Whether the pool was successfully removed.</returns>
		public bool RemovePool(Pool pool)
		{
			bool result = Pools.Remove(pool);
			if (result)
			{
				// pool.Group = null;
				pool.transform.parent = null;
			}

			return result;
		}

		#endregion

		#region Helper methods

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Pools.GetEnumerator();
		}

		IEnumerator<Pool> IEnumerable<Pool>.GetEnumerator()
		{
			return Pools.GetEnumerator();
		}
		#endregion

		#region Editor functionality

#if UNITY_EDITOR
		private void AddPool_Editor()
		{
			GameObject poolGO = new GameObject("Pool " + (transform.childCount + 1));
			Pool pool = poolGO.AddComponent<Pool>();
			AddPool(pool);
			pool.MessageMode = MessageMode;
			pool.Organize = Organize;
		}

		private void DestroyPool_Editor(int index)
		{
			Pool pool = Pools[index];
			if (pool == null)
			{
				Pools.RemoveAt(index);
				return;
			}

			RemovePool(pool);
			DestroyImmediate(pool.gameObject);
		}

		private void ResetMessageMode()
		{
			foreach (Pool pool in Pools)
				pool.MessageMode = MessageMode;
		}

		private void ResetOrganize()
		{
			foreach (Pool pool in Pools)
				pool.Organize = Organize;
		}

		private bool ShowPersistent => transform.parent == null;
#endif

		#endregion

		#region Public properties

		/// <summary>All the instances available for re-use in this group.</summary>
		public IEnumerable<Component> Available => Pools.SelectMany(p => p.Available);

		/// <summary>Total number of instances available for re-use in this group.</summary>
		public int AvailableCount => Pools.Sum(p => p.Available.Count);

		/// <summary>All the instances being used by pools in this group.</summary>
		public IEnumerable<Component> Used => Pools.SelectMany(p => p.Used);

		/// <summary>Total number of instances being used by pools in this group.</summary>
		public int UsedCount => Pools.Sum(p => p.Used.Count);

		#endregion
	}
}