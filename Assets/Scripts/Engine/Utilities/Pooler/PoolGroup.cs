using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

public class PoolGroup : MonoBehaviour, IEnumerable<Component>
{
	public bool Organize = true;

	[PropertySpace]

	[InlineEditor]
	[SceneObjectsOnly]
	[ListDrawerSettings(CustomAddFunction = "AddPool", CustomRemoveElementFunction = "DestroyPool")]
	public List<Pool> Pools = new List<Pool>();

	protected void Awake()
	{
		Pooler.CachePoolGroup(this);
	}

	protected void OnDestroy()
	{
		Pooler.UncachePoolGroup(this);
	}

	public void AddPool(Pool pool)
	{
		Pools.Add(pool);
		pool.Group = this;
		if (Organize)
			pool.transform.parent = transform;
	}

	public bool ContainsPool(Pool pool)
	{
		return Pools.Contains(pool);
	}

	public bool RemovePool(Pool pool)
	{
		pool.Group = null;
		if (Organize)
			pool.transform.parent = null;
		return Pools.Remove(pool);
	}

	public IEnumerable<Component> Available
	{
		get
		{
			return Pools.SelectMany(p => p.Available);
		}
	}

	public int AvailableCount
	{
		get
		{
			return Pools.Sum(p => p.Available.Count);
		}
	}

	public IEnumerable<Component> Used
	{
		get
		{
			return Pools.SelectMany(p => p.Used);
		}
	}

	public int UsedCount
	{
		get
		{
			return Pools.Sum(p => p.Used.Count);
		}
	}

	protected void AddPool()
	{
		string name = "Pool " + (transform.childCount + 1);
		GameObject poolGO = new GameObject(name);
		Pool pool = poolGO.AddComponent<Pool>();
		AddPool(pool);
	}

	protected void DestroyPool(Pool pool)
	{
		RemovePool(pool);
		DestroyImmediate(pool.gameObject);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return Used.GetEnumerator();
	}

	public IEnumerator<Component> GetEnumerator()
	{
		return Used.GetEnumerator();
	}
}