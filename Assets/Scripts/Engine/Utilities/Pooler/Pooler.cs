using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Decide between Pooler -> Pool or Pooler -> PoolGroup -> Pool
//		 You can keep track of all instances of a type if you use PoolGroup but they are more complicated
//		 Preloading would also not be as easy without PoolGroup as you would have to create Pools in the scene for every prefab
// TODO: Make work with Particle Systems, UIs and AudioSources
// TODO: Make stable (can't assign individual scripts as prefab...)
// TODO: Make fault-tolarent

public class Pooler
{
	protected static Dictionary<string, Pool> poolsByName = new Dictionary<string, Pool>();
	protected static Dictionary<Component, Pool> poolsByPrefab = new Dictionary<Component, Pool>();

	#region Pool Management
	public static void CachePool(Pool pool)
	{
		if (pool.Prefab == null)
			return;

		poolsByPrefab[pool.Prefab] = pool;
		poolsByName[pool.name] = pool;
	}

	public static void UncachePool(Pool pool)
	{
		poolsByPrefab.Remove(pool.Prefab);
		poolsByName.Remove(pool.name);
	}

	public static Pool CreatePool(Component prefab)
	{
		return CreatePool(prefab.name, prefab);
	}

	public static Pool CreatePool(string name, Component prefab)
	{
		GameObject poolGO = new GameObject(name);
		Pool pool = poolGO.AddComponent<Pool>();
		pool.Prefab = prefab;
		poolsByName.Add(name, pool);
		poolsByPrefab.Add(prefab, pool);
		return pool;
	}

	public static bool ContainsPool(Component prefab)
	{
		return poolsByPrefab.ContainsKey(prefab);
	}

	public static bool ContainsPool(string name)
	{
		return poolsByName.ContainsKey(name);
	}

	public static Pool GetPool(Component prefab)
	{
		if (poolsByPrefab.TryGetValue(prefab, out Pool pool))
			return pool;
		return null;
	}

	public static Pool GetPool(string name)
	{
		if (poolsByName.TryGetValue(name, out Pool pool))
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

	public static bool DestroyPool(string name)
	{
		Pool pool = GetPool(name);
		if (pool != null)
		{
			pool.Destroy();
			return true;
		}
		return false;
	}

	public static bool DestroyPool(Component prefab)
	{
		Pool pool = GetPool(prefab);
		if (pool != null)
		{
			pool.Destroy();
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

	public static bool Destroy(Component instance)
	{
		return Destroy(instance.name, instance);
	}

	public static bool Destroy(string name, Component instance)
	{
		Pool pool = GetPool(name);
		if (pool != null)
		{
			pool.Destroy(instance);
			return true;
		}
		return false;
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