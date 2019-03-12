using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Engine.Pooling
{
	[AddComponentMenu("Pooling/PoolGroup")]
	public class PoolGroup : MonoBehaviour, IEnumerable<Component>
	{
		#region Fields
		[LabelText("Message")]
		[OnValueChanged("ResetMessageMode")]
		public PoolMessageMode MessageMode = PoolMessageMode.None;

		[OnValueChanged("ResetOrganize")]
		public bool Organize = true;

		[ShowIf("ShowPersistent")]
		public bool Persistent = false;

		[PropertySpace]

		[SceneObjectsOnly]
		[InlineEditor]
		[ListDrawerSettings(CustomAddFunction = "AddPool", CustomRemoveElementFunction = "DestroyPool")]
		public List<Pool> Pools = new List<Pool>();

		public bool IsDestroying { get; protected set; }
		#endregion

		#region Initialization
		protected void Awake()
		{
			Pooler.CacheGroup(this);
		}

		protected void Start()
		{
			if (Persistent && transform.parent == null)
				DontDestroyOnLoad(gameObject);
		}

		protected void OnDestroy()
		{
			IsDestroying = true;
			Pooler.UncacheGroup(this);
		}
		#endregion

		#region Pool management
		public void AddPool(Pool pool)
		{
			Pools.Add(pool);
			pool.transform.parent = transform;

			pool.Group = this;
			pool.MessageMode = MessageMode;
			pool.Organize = Organize;
			pool.Persistent = false;
		}

		public bool ContainsPool(Pool pool)
		{
			return Pools.Contains(pool);
		}

		public bool RemovePool(Pool pool)
		{
			pool.Group = null;
			pool.transform.parent = null;
			return Pools.Remove(pool);
		}
		#endregion

		#region Helper methods
		IEnumerator IEnumerable.GetEnumerator()
		{
			return Used.GetEnumerator();
		}

		public IEnumerator<Component> GetEnumerator()
		{
			return Used.GetEnumerator();
		}
		#endregion

		#region Editor functionality
#if UNITY_EDITOR
		private void AddPool()
		{
			GameObject poolGO = new GameObject("Pool " + (transform.childCount + 1));
			Pool pool = poolGO.AddComponent<Pool>();
			AddPool(pool);
		}

		private void DestroyPool(Pool pool)
		{
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
		public IEnumerable<Component> Available => Pools.SelectMany(p => p.Available);
		public int AvailableCount => Pools.Sum(p => p.Available.Count);

		public IEnumerable<Component> Used => Pools.SelectMany(p => p.Used);
		public int UsedCount => Pools.Sum(p => p.Used.Count);

		#endregion
	}
}