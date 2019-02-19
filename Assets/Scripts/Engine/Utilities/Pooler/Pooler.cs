using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Pooler
{
	private static Dictionary<string, PoolGroup> poolGroupsByName = new Dictionary<string, PoolGroup>();

	private static Dictionary<string, Pool> poolsByName = new Dictionary<string, Pool>();
	private static Dictionary<Component, Pool> poolsByPrefab = new Dictionary<Component, Pool>();

	#region PoolGroup management
	public static void CacheGroup(PoolGroup group)
	{
		poolGroupsByName.Add(group.name, group);
	}

	public static void UncacheGroup(PoolGroup group)
	{
		poolGroupsByName.Remove(group.name);
	}

	public static PoolGroup CreateGroup(string name)
	{
		GameObject groupGO = new GameObject(name);
		return groupGO.AddComponent<PoolGroup>();
	}

	public static PoolGroup GetOrCreateGroup(string name)
	{
		PoolGroup group = GetGroup(name);
		if (group != null)
			return group;
		return CreateGroup(name);
	}

	public static PoolGroup GetGroup(string name)
	{
		if (poolGroupsByName.TryGetValue(name, out PoolGroup group))
			return group;
		return null;
	}

	public static bool DestroyGroup(string name)
	{
		PoolGroup group = GetGroup(name);
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

	public static void AddPoolToGroup(string group, Pool pool)
	{
		PoolGroup groupInstance = GetGroup(group);
		if (groupInstance != null)
			AddPoolToGroup(groupInstance, pool);
	}

	public static void RemovePoolFromGroup(PoolGroup group, Pool pool)
	{
		group.RemovePool(pool);
	}

	public static void RemovePoolFromGroup(string group, Pool pool)
	{
		PoolGroup groupInstance = GetGroup(group);
		if (groupInstance != null)
			RemovePoolFromGroup(groupInstance, pool);
	}
	#endregion

	#region Pool management
	public static void CachePool(Pool pool)
	{
		if (pool.Prefab != null)
			poolsByPrefab.Add(pool.Prefab, pool);
		poolsByName.Add(pool.name, pool);
	}

	public static void UncachePool(Pool pool)
	{
		if (pool.Prefab != null)
			poolsByPrefab.Remove(pool.Prefab);
		poolsByName.Remove(pool.name);
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

	public static bool ContainsPool(string name)
	{
		return poolsByName.ContainsKey(name);
	}

	public static bool ContainsPool(Component prefab)
	{
		return poolsByPrefab.ContainsKey(prefab);
	}

	public static Pool GetPool(string name)
	{
		if (poolsByName.TryGetValue(name, out Pool pool))
			return pool;
		return null;
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
			GetOrCreateGroup(group).AddPool(pool);
		}
		return pool;
	}

	public static bool DestroyPool(string name)
	{
		Pool pool = GetPool(name);
		if (pool != null)
		{
			pool.gameObject.Destroy();
			return true;
		}
		return false;
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
	public static Component Instantiate(string name)
	{
		return GetPool(name)?.Instantiate();
	}

	public static Component Instantiate(string name, Transform parent, bool worldPositionStays = false)
	{
		return GetPool(name)?.Instantiate(parent, worldPositionStays);
	}

	public static Component Instantiate(string name, Vector3 position)
	{
		return GetPool(name)?.Instantiate(position);
	}

	public static Component Instantiate(string name, Vector3 position, Quaternion rotation)
	{
		return GetPool(name)?.Instantiate(position, rotation);
	}

	public static Component Instantiate(string name, Vector3 position, Quaternion rotation, Transform parent)
	{
		return GetPool(name)?.Instantiate(position, rotation, parent);
	}

	public static T Instantiate<T>(string name) where T : Component
	{
		return GetPool(name)?.Instantiate<T>();
	}

	public static T Instantiate<T>(string name, Transform parent, bool worldPositionStays = false) where T : Component
	{
		return GetPool(name)?.Instantiate<T>(parent, worldPositionStays);
	}

	public static T Instantiate<T>(string name, Vector3 position) where T : Component
	{
		return GetPool(name)?.Instantiate<T>(position);
	}

	public static T Instantiate<T>(string name, Vector3 position, Quaternion rotation) where T : Component
	{
		return GetPool(name)?.Instantiate<T>(position, rotation);
	}

	public static T Instantiate<T>(string name, Vector3 position, Quaternion rotation, Transform parent) where T : Component
	{
		return GetPool(name)?.Instantiate<T>(position, rotation, parent);
	}

	public static Component Instantiate(Component prefab)
	{
		return GetOrCreatePool(prefab).Instantiate();
	}

	public static Component Instantiate(Component prefab, Transform parent, bool worldPositionStays = false)
	{
		return GetOrCreatePool(prefab).Instantiate(parent, worldPositionStays);
	}

	public static Component Instantiate(Component prefab, Vector3 position)
	{
		return GetOrCreatePool(prefab).Instantiate(position);
	}

	public static Component Instantiate(Component prefab, Vector3 position, Quaternion rotation)
	{
		return GetOrCreatePool(prefab).Instantiate(position, rotation);
	}

	public static Component Instantiate(Component prefab, Vector3 position, Quaternion rotation, Transform parent)
	{
		return GetOrCreatePool(prefab).Instantiate(position, rotation, parent);
	}

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
		PoolInstance poolInstance = instance.GetComponent<PoolInstance>();
		if (poolInstance?.Pool != null)
		{
			poolInstance.Pool.Destroy(instance);
			return true;
		}
		return false;
	}

	public static bool DestroyAll(Component prefab)
	{
		Pool pool = GetPool(prefab);
		if (pool != null)
		{
			pool.DestroyAll();
			return true;
		}
		return false;
	}

	public static bool DestroyAll(string name)
	{
		Pool pool = GetPool(name);
		if (pool != null)
		{
			pool.DestroyAll();
			return true;
		}
		return false;
	}

	public static bool DestroyAllInGroup(string name)
	{
		PoolGroup groupInstance = GetGroup(name);
		if (groupInstance != null)
		{
			foreach (Pool pool in groupInstance.Pools)
				pool.DestroyAll();
			return true;
		}
		return false;
	}
	#endregion
}