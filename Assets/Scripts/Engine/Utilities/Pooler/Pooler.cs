using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Decide between Pooler -> Pool or Pooler -> PoolGroup -> Pool
//		 You can keep track of all instances of a type if you use PoolGroup but they are more complicated
//		 Preloading would also not be as easy without PoolGroup as you would have to create Pools in the scene for every prefab
// TODO: Manage and return as T
// TODO: Make work with Particle Systems, UIs and AudioSources
// TODO: Allow to cull instances (remove and destroy objects if there are too many pooled instances
//		-- can happen if too many objects are spawned and despawned during gameplay spikes)
// TODO: Allow to limit instances (return null in Spawn when limit is reached) (limitAmount)
// TODO: Allow to limit instances by despawning first-created object and re-using it (limitFIFO)

public class Pooler
{
	protected static Dictionary<string, Pool> poolsByName = new Dictionary<string, Pool>();
	protected static Dictionary<Component, Pool> poolsByPrefab = new Dictionary<Component, Pool>();

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

	public static Component Spawn(string name)
	{
		return GetPool(name)?.Spawn();
	}

	public static Component Spawn(Component prefab)
	{
		return GetOrCreatePool(prefab).Spawn();
	}

	public static T Spawn<T>(string name) where T : Component
	{
		return GetPool(name)?.Spawn<T>();
	}

	public static T Spawn<T>(Component prefab) where T : Component
	{
		return GetOrCreatePool(prefab).Spawn<T>();
	}

	public static bool Despawn(Component instance)
	{
		return Despawn(instance.name, instance);
	}

	public static bool Despawn(string name, Component instance)
	{
		Pool pool = GetPool(name);
		if (pool != null)
		{
			pool.Despawn(instance);
			return true;
		}
		return false;
	}

	public static bool Despawn(Component prefab, Component instance)
	{
		Pool pool = GetPool(prefab);
		if (pool != null)
		{
			pool.Despawn(instance);
			return true;
		}
		return false;
	}

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
}