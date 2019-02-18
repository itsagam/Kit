using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Solve collision between Pools with same prefab name
// TODO: Modify settings with PoolGroup
// TODO: Make stable (can't assign individual scripts as prefab...)
// TODO: Make fault-tolarent (external destroyed instances...)
// TODO: Make work with Particle Systems, UIs and AudioSources

public static class Pooler
{
	private static Dictionary<string, PoolGroup> poolGroupsByName = new Dictionary<string, PoolGroup>();

	private static Dictionary<Component, Pool> poolsByPrefab = new Dictionary<Component, Pool>();

	#region PoolGroup management
	public static void CachePoolGroup(PoolGroup group)
	{
		if (group != null)
			poolGroupsByName[group.name] = group;
	}

	public static void UncachePoolGroup(PoolGroup group)
	{
		if (group != null)
			poolGroupsByName.Remove(group.name);
	}

	public static PoolGroup CreatePoolGroup(string name)
	{
		GameObject groupGO = new GameObject(name);
		var group = groupGO.AddComponent<PoolGroup>();
		poolGroupsByName.Add(name, group);
		return group;
	}

	public static PoolGroup GetOrCreatePoolGroup(string name)
	{
		var group = GetPoolGroup(name);
		if (group != null)
			return group;
		return CreatePoolGroup(name);
	}

	public static PoolGroup GetPoolGroup(string name)
	{
		if (poolGroupsByName.TryGetValue(name, out PoolGroup group))
			return group;
		return null;
	}

	public static bool DestroyGroup(string name)
	{
		var group = GetPoolGroup(name);
		if (group != null)
		{
			group.gameObject.Destroy();
			return true;
		}
		return false;
	}

	public static void AddPoolToGroup(PoolGroup group, Pool pool)
	{
		group.AddPool(pool);
	}

	public static void AddPoolToGroup(string groupName, Pool pool)
	{
		var group = GetPoolGroup(groupName);
		if (group != null)
			AddPoolToGroup(group, pool);
	}

	public static void RemovePoolFromGroup(PoolGroup group, Pool pool)
	{
		group.RemovePool(pool);
	}

	public static void RemovePoolfromGroup(string groupName, Pool pool)
	{
		var group = GetPoolGroup(groupName);
		if (group != null)
			RemovePoolFromGroup(group, pool);
	}
	#endregion

	#region Pool management
	public static void CachePool(Pool pool)
	{
		if (pool?.Prefab != null)
			poolsByPrefab[pool.Prefab] = pool;
	}

	public static void UncachePool(Pool pool)
	{
		if (pool?.Prefab != null)
			poolsByPrefab.Remove(pool.Prefab);
	}

	public static Pool CreatePool(Component prefab)
	{
		return CreatePool(prefab, prefab.name);
	}

	public static Pool CreatePool(Component prefab, string name)
	{
		GameObject poolGO = new GameObject(name);
		Pool pool = poolGO.AddComponent<Pool>();
		pool.Prefab = prefab;
		poolsByPrefab.Add(prefab, pool);
		return pool;
	}

	public static bool ContainsPool(Component prefab)
	{
		return poolsByPrefab.ContainsKey(prefab);
	}

	public static Pool GetPool(Component prefab)
	{
		if (poolsByPrefab.TryGetValue(prefab, out Pool pool))
			return pool;
		return null;
	}

	public static Pool GetOrCreatePool(Component prefab)
	{
		Pool pool = GetPool(prefab);
		if (pool == null)
			pool = CreatePool(prefab);
		return pool;
	}

	public static Pool GetOrCreatePool(string group, Component prefab)
	{
		Pool pool = GetPool(prefab);
		if (pool == null)
		{
			pool = CreatePool(prefab);
			GetOrCreatePoolGroup(group).AddPool(pool);
		}
		return pool;
	}

	public static bool DestroyPool(Component prefab)
	{
		Pool pool = GetPool(prefab);
		if (pool != null)
		{
			pool.gameObject.Destroy();
			return true;
		}
		return false;
	}
	#endregion

	#region Instantiate/Destroy
	public static T Instantiate<T>(T prefab) where T : Component
	{
		return GetOrCreatePool(prefab).Instantiate<T>();
	}

	public static T Instantiate<T>(T prefab, Transform parent, bool worldPositionStays = false) where T : Component
	{
		return GetOrCreatePool(prefab).Instantiate<T>(parent, worldPositionStays);
	}

	public static T Instantiate<T>(T prefab, Vector3 position) where T : Component
	{
		return GetOrCreatePool(prefab).Instantiate<T>(position);
	}

	public static T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
	{
		return GetOrCreatePool(prefab).Instantiate<T>(position, rotation);
	}

	public static T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Component
	{
		return GetOrCreatePool(prefab).Instantiate<T>(position, rotation, parent);
	}

	public static T Instantiate<T>(string group, T prefab) where T : Component
	{
		return GetOrCreatePool(group, prefab).Instantiate<T>();
	}

	public static T Instantiate<T>(string group, T prefab, Transform parent, bool worldPositionStays = false) where T : Component
	{
		return GetOrCreatePool(group, prefab).Instantiate<T>(parent, worldPositionStays);
	}

	public static T Instantiate<T>(string group, T prefab, Vector3 position) where T : Component
	{
		return GetOrCreatePool(group, prefab).Instantiate<T>(position);
	}

	public static T Instantiate<T>(string group, T prefab, Vector3 position, Quaternion rotation) where T : Component
	{
		return GetOrCreatePool(group, prefab).Instantiate<T>(position, rotation);
	}

	public static T Instantiate<T>(string group, T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Component
	{
		return GetOrCreatePool(group, prefab).Instantiate<T>(position, rotation, parent);
	}

	public static bool Destroy(Component instance)
	{
		return false;
		//return Destroy(instance.name, instance);
	}

	public static bool Destroy(Component prefab, Component instance)
	{
		Pool pool = GetPool(prefab);
		if (pool != null)
		{
			pool.Destroy(instance);
			return true;
		}
		return false;
	}
	#endregion
}