using System.Collections.Generic;
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
		return group != null ? group : CreateGroup(name);
	}

	public static PoolGroup GetGroup(string name)
	{
		return poolGroupsByName.TryGetValue(name, out PoolGroup group) ? @group : null;
	}

	public static bool DestroyGroup(string name)
	{
		PoolGroup group = GetGroup(name);
		if (group == null)
			return false;
		group.gameObject.Destroy();
		return true;
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
		return poolsByName.TryGetValue(name, out Pool pool) ? pool : null;
	}

	public static Pool GetPool(Component prefab)
	{
		return poolsByPrefab.TryGetValue(prefab, out Pool pool) ? pool : null;
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
		if (pool != null)
			return pool;
		pool = CreatePool(prefab);
		GetOrCreateGroup(@group).AddPool(pool);
		return pool;
	}

	public static Pool GetOrCreatePool(PoolGroup group, Component prefab)
	{
		Pool pool = GetPool(prefab);
		if (pool != null)
			return pool;
		pool = CreatePool(prefab);
		group.AddPool(pool);
		return pool;
	}

	public static bool DestroyPool(string name)
	{
		Pool pool = GetPool(name);
		if (pool == null)
			return false;
		pool.gameObject.Destroy();
		return true;
	}

	public static bool DestroyPool(Component prefab)
	{
		Pool pool = GetPool(prefab);
		if (pool == null)
			return false;
		pool.gameObject.Destroy();
		return true;
	}
	#endregion

	#region Instantiate/Destroy
	public static Component Instantiate(string name)
	{
		return GetPool(name)?.Instantiate();
	}

	public static Component Instantiate(string name, Transform parent, bool worldSpace = false)
	{
		return GetPool(name)?.Instantiate(parent, worldSpace);
	}

	public static Component Instantiate(string name, Vector3 position)
	{
		return GetPool(name)?.Instantiate(position);
	}

	public static Component Instantiate(string name, Vector3 position, Quaternion rotation)
	{
		return GetPool(name)?.Instantiate(position, rotation);
	}

	public static Component Instantiate(string name, Vector3 position, Transform parent)
	{
		return GetPool(name)?.Instantiate(position, parent);
	}

	public static Component Instantiate(string name, Vector3 position, Quaternion rotation, Transform parent)
	{
		return GetPool(name)?.Instantiate(position, rotation, parent);
	}

	public static T Instantiate<T>(string name) where T : Component
	{
		return GetPool(name)?.Instantiate<T>();
	}

	public static T Instantiate<T>(string name, Transform parent, bool worldSpace = false) where T : Component
	{
		return GetPool(name)?.Instantiate<T>(parent, worldSpace);
	}

	public static T Instantiate<T>(string name, Vector3 position) where T : Component
	{
		return GetPool(name)?.Instantiate<T>(position);
	}

	public static T Instantiate<T>(string name, Vector3 position, Quaternion rotation) where T : Component
	{
		return GetPool(name)?.Instantiate<T>(position, rotation);
	}

	public static T Instantiate<T>(string name, Vector3 position, Transform parent) where T : Component
	{
		return GetPool(name)?.Instantiate<T>(position, parent);
	}

	public static T Instantiate<T>(string name, Vector3 position, Quaternion rotation, Transform parent) where T : Component
	{
		return GetPool(name)?.Instantiate<T>(position, rotation, parent);
	}

	public static Component Instantiate(Component prefab)
	{
		return GetOrCreatePool(prefab).Instantiate();
	}

	public static Component Instantiate(Component prefab, Transform parent, bool worldSpace = false)
	{
		return GetOrCreatePool(prefab).Instantiate(parent, worldSpace);
	}

	public static Component Instantiate(Component prefab, Vector3 position)
	{
		return GetOrCreatePool(prefab).Instantiate(position);
	}

	public static Component Instantiate(Component prefab, Vector3 position, Quaternion rotation)
	{
		return GetOrCreatePool(prefab).Instantiate(position, rotation);
	}

	public static Component Instantiate(Component prefab, Vector3 position, Transform parent)
	{
		return GetOrCreatePool(prefab).Instantiate(position, parent);
	}

	public static Component Instantiate(Component prefab, Vector3 position, Quaternion rotation, Transform parent)
	{
		return GetOrCreatePool(prefab).Instantiate(position, rotation, parent);
	}

	public static T Instantiate<T>(T prefab) where T : Component
	{
		return GetOrCreatePool(prefab).Instantiate<T>();
	}

	public static T Instantiate<T>(T prefab, Transform parent, bool worldSpace = false) where T : Component
	{
		return GetOrCreatePool(prefab).Instantiate<T>(parent, worldSpace);
	}

	public static T Instantiate<T>(T prefab, Vector3 position) where T : Component
	{
		return GetOrCreatePool(prefab).Instantiate<T>(position);
	}

	public static T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
	{
		return GetOrCreatePool(prefab).Instantiate<T>(position, rotation);
	}

	public static T Instantiate<T>(T prefab, Vector3 position, Transform parent) where T : Component
	{
		return GetOrCreatePool(prefab).Instantiate<T>(position, parent);
	}

	public static T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Component
	{
		return GetOrCreatePool(prefab).Instantiate<T>(position, rotation, parent);
	}

	public static T Instantiate<T>(string group, T prefab) where T : Component
	{
		return GetOrCreatePool(group, prefab).Instantiate<T>();
	}

	public static T Instantiate<T>(string group, T prefab, Transform parent, bool worldSpace = false) where T : Component
	{
		return GetOrCreatePool(group, prefab).Instantiate<T>(parent, worldSpace);
	}

	public static T Instantiate<T>(string group, T prefab, Vector3 position) where T : Component
	{
		return GetOrCreatePool(group, prefab).Instantiate<T>(position);
	}

	public static T Instantiate<T>(string group, T prefab, Vector3 position, Quaternion rotation) where T : Component
	{
		return GetOrCreatePool(group, prefab).Instantiate<T>(position, rotation);
	}

	public static T Instantiate<T>(string group, T prefab, Vector3 position,  Transform parent) where T : Component
	{
		return GetOrCreatePool(group, prefab).Instantiate<T>(position, parent);
	}

	public static T Instantiate<T>(string group, T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Component
	{
		return GetOrCreatePool(group, prefab).Instantiate<T>(position, rotation, parent);
	}

	public static T Instantiate<T>(PoolGroup group, T prefab) where T : Component
	{
		return GetOrCreatePool(group, prefab).Instantiate<T>();
	}

	public static T Instantiate<T>(PoolGroup group, T prefab, Transform parent, bool worldSpace = false) where T : Component
	{
		return GetOrCreatePool(group, prefab).Instantiate<T>(parent, worldSpace);
	}

	public static T Instantiate<T>(PoolGroup group, T prefab, Vector3 position) where T : Component
	{
		return GetOrCreatePool(group, prefab).Instantiate<T>(position);
	}

	public static T Instantiate<T>(PoolGroup group, T prefab, Vector3 position, Quaternion rotation) where T : Component
	{
		return GetOrCreatePool(group, prefab).Instantiate<T>(position, rotation);
	}

	public static T Instantiate<T>(PoolGroup group, T prefab, Vector3 position, Transform parent) where T : Component
	{
		return GetOrCreatePool(group, prefab).Instantiate<T>(position, parent);
	}

	public static T Instantiate<T>(PoolGroup group, T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Component
	{
		return GetOrCreatePool(group, prefab).Instantiate<T>(position, rotation, parent);
	}

	public static bool Destroy(Component instance)
	{
		if (instance == null)
			return false;

		PoolInstance poolInstance = instance.GetComponent<PoolInstance>();
		if (poolInstance?.Pool == null)
			return false;

		poolInstance.Pool.Destroy(instance);
		return true;
	}

	public static bool DestroyAll(Component prefab)
	{
		Pool pool = GetPool(prefab);
		if (pool == null)
			return false;
		pool.DestroyAll();
		return true;
	}

	public static bool DestroyAll(string name)
	{
		Pool pool = GetPool(name);
		if (pool == null)
			return false;
		pool.DestroyAll();
		return true;
	}

	public static bool DestroyAllInGroup(string name)
	{
		PoolGroup group = GetGroup(name);
		if (group == null)
			return false;
		foreach (Pool pool in group.Pools)
			pool.DestroyAll();
		return true;
	}
	#endregion
}