﻿using System.Collections.Generic;
using UnityEngine;

namespace Kit.Pooling
{
	/// <summary>A robust and easy-to-use <see cref="GameObject" /> pooler. Supports grouping, limiting and pre-loading.</summary>
	/// <example>
	///     Replace <see cref="Object.Instantiate(Object)" qualifyHint="true" /> calls with
	///     <see cref="Pooler.Instantiate(Component)" qualifyHint="true" /> &amp;
	///     <see cref="Object.Destroy(Object)" qualifyHint="true" /> with <see cref="Pooler.Destroy(Component)" qualifyHint="true" />,
	///     and you're good to go.
	/// </example>
	public static class Pooler
	{
		#region Fields

		private static Dictionary<string, PoolGroup> poolGroupsByName = new Dictionary<string, PoolGroup>();

		private static Dictionary<string, Pool> poolsByName = new Dictionary<string, Pool>();
		private static Dictionary<Component, Pool> poolsByPrefab = new Dictionary<Component, Pool>();

		private static Dictionary<GameObject, PoolInstanceInfo> poolsByGameObject = new Dictionary<GameObject, PoolInstanceInfo>();

		#endregion

		#region PoolGroup management

		/// <summary>Register a group. Called automatically.</summary>
		public static void CacheGroup(PoolGroup group)
		{
			poolGroupsByName.Add(group.name, group);
		}

		/// <summary>Unregister a group. Called automatically.</summary>
		public static void UncacheGroup(PoolGroup group)
		{
			poolGroupsByName.Remove(group.name);
		}

		/// <summary>Create a new pool group.</summary>
		public static PoolGroup CreateGroup(string name)
		{
			GameObject groupGO = new GameObject(name);
			return groupGO.AddComponent<PoolGroup>();
		}

		/// <summary>Get a pool group or create it if it doesn't exist.</summary>
		public static PoolGroup GetOrCreateGroup(string name)
		{
			PoolGroup group = GetGroup(name);
			return group != null ? group : CreateGroup(name);
		}

		/// <summary>Get a pool group.</summary>
		public static PoolGroup GetGroup(string name)
		{
			return poolGroupsByName.GetOrDefault(name);
		}

		/// <summary>Destroy a pool group.</summary>
		public static bool DestroyGroup(string name)
		{
			PoolGroup group = GetGroup(name);
			if (group == null)
				return false;
			group.gameObject.Destroy();
			return true;
		}

		/// <summary>Add a particular pool to a group.</summary>
		public static void AddPoolToGroup(PoolGroup group, Pool pool)
		{
			group.AddPool(pool);
		}

		/// <summary>Add a particular pool to a group.</summary>
		public static void AddPoolToGroup(string group, Pool pool)
		{
			PoolGroup groupInstance = GetGroup(group);
			if (groupInstance != null)
				AddPoolToGroup(groupInstance, pool);
		}

		/// <summary>Remove a particular pool from a group.</summary>
		public static void RemovePoolFromGroup(PoolGroup group, Pool pool)
		{
			group.RemovePool(pool);
		}

		/// <summary>Remove a particular pool from a group.</summary>
		public static void RemovePoolFromGroup(string group, Pool pool)
		{
			PoolGroup groupInstance = GetGroup(group);
			if (groupInstance != null)
				RemovePoolFromGroup(groupInstance, pool);
		}

		#endregion

		#region Pool management

		/// <summary>Register a pool. Called automatically.</summary>
		public static void CachePool(Pool pool)
		{
			if (pool.Prefab != null)
				poolsByPrefab.Add(pool.Prefab, pool);
			poolsByName.Add(pool.name, pool);
		}

		/// <summary>Unregister a pool. Called automatically.</summary>
		public static void UncachePool(Pool pool)
		{
			if (pool.Prefab != null)
				poolsByPrefab.Remove(pool.Prefab);
			poolsByName.Remove(pool.name);
		}

		/// <summary>Create a new pool for a prefab.</summary>
		/// <param name="prefab">Prefab to create the pool for.</param>
		public static Pool CreatePool(Component prefab)
		{
			return CreatePool(prefab, prefab.name);
		}

		/// <summary>Create a new pool for a prefab.</summary>
		/// <param name="prefab">Prefab to create the pool for.</param>
		/// <param name="name">Name of the game object for the pool.</param>
		public static Pool CreatePool(Component prefab, string name)
		{
			GameObject poolGO = new GameObject(name);
			Pool pool = poolGO.AddComponent<Pool>();
			pool.Prefab = prefab;
			poolsByPrefab.Add(prefab, pool);
			return pool;
		}

		/// <summary>Returns whether a particular pool exists.</summary>
		public static bool ContainsPool(string name)
		{
			return poolsByName.ContainsKey(name);
		}

		/// <summary>Returns whether a particular pool exists.</summary>
		public static bool ContainsPool(Component prefab)
		{
			return poolsByPrefab.ContainsKey(prefab);
		}

		/// <summary>Get a particular pool.</summary>
		public static Pool GetPool(string name)
		{
			return poolsByName.GetOrDefault(name);
		}

		/// <summary>Get a particular pool.</summary>
		public static Pool GetPool(Component prefab)
		{
			return poolsByPrefab.GetOrDefault(prefab);
		}

		/// <summary>Get a pool or create it if it doesn't exist.</summary>
		public static Pool GetOrCreatePool(Component prefab)
		{
			Pool pool = GetPool(prefab);
			if (pool == null)
				pool = CreatePool(prefab);
			return pool;
		}

		/// <summary>Get a pool or create it in a particular group if it doesn't exist.</summary>
		public static Pool GetOrCreatePool(string group, Component prefab)
		{
			Pool pool = GetPool(prefab);
			if (pool != null)
				return pool;
			pool = CreatePool(prefab);
			GetOrCreateGroup(group).AddPool(pool);
			return pool;
		}

		/// <summary>Get a pool or create it in a particular group if it doesn't exist.</summary>
		public static Pool GetOrCreatePool(PoolGroup group, Component prefab)
		{
			Pool pool = GetPool(prefab);
			if (pool != null)
				return pool;
			pool = CreatePool(prefab);
			group.AddPool(pool);
			return pool;
		}

		/// <summary>Destroy a pool.</summary>
		public static bool DestroyPool(string name)
		{
			Pool pool = GetPool(name);
			if (pool == null)
				return false;
			pool.gameObject.Destroy();
			return true;
		}

		/// <summary>Destroy a pool.</summary>
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

		/// <summary>
		/// Register a pool instance. Called automatically.
		/// </summary>
		public static void CacheInstance(GameObject gameObject, PoolInstanceInfo info)
		{
			poolsByGameObject.Add(gameObject, info);
		}

		/// <summary>
		/// Get information about a pool instance.
		/// </summary>
		/// <returns>An instance of the <see cref="PoolInstanceInfo"/> class.</returns>
		public static PoolInstanceInfo GetInstanceInfo(Component component)
		{
			return GetInstanceInfo(component.gameObject);
		}

		/// <summary>
		/// Get information about a pool instance.
		/// </summary>
		/// <returns>An instance of the <see cref="PoolInstanceInfo"/> class.</returns>
		public static PoolInstanceInfo GetInstanceInfo(GameObject gameObject)
		{
			return poolsByGameObject[gameObject];
		}

		/// <summary>
		/// Unregister a pool instance. Called automatically.
		/// </summary>
		public static void UncacheInstance(GameObject gameObject)
		{
			poolsByGameObject.Remove(gameObject);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance created.</returns>
		public static Component Instantiate(string pool)
		{
			return GetPool(pool)?.Instantiate();
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance created.</returns>
		public static Component Instantiate(string pool, Transform parent, bool worldSpace = false)
		{
			return GetPool(pool)?.Instantiate(parent, worldSpace);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static Component Instantiate(string pool, Vector3 position)
		{
			return GetPool(pool)?.Instantiate(position);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static Component Instantiate(string pool, Vector3 position, Quaternion rotation)
		{
			return GetPool(pool)?.Instantiate(position, rotation);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static Component Instantiate(string pool, Vector3 position, Transform parent)
		{
			return GetPool(pool)?.Instantiate(position, parent);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static Component Instantiate(string pool, Vector3 position, Quaternion rotation, Transform parent)
		{
			return GetPool(pool)?.Instantiate(position, rotation, parent);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static T Instantiate<T>(string pool) where T: Component
		{
			return GetPool(pool)?.Instantiate<T>();
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static T Instantiate<T>(string pool, Transform parent, bool worldSpace = false) where T: Component
		{
			return GetPool(pool)?.Instantiate<T>(parent, worldSpace);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static T Instantiate<T>(string pool, Vector3 position) where T: Component
		{
			return GetPool(pool)?.Instantiate<T>(position);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static T Instantiate<T>(string pool, Vector3 position, Quaternion rotation) where T: Component
		{
			return GetPool(pool)?.Instantiate<T>(position, rotation);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static T Instantiate<T>(string pool, Vector3 position, Transform parent) where T: Component
		{
			return GetPool(pool)?.Instantiate<T>(position, parent);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static T Instantiate<T>(string pool, Vector3 position, Quaternion rotation, Transform parent) where T: Component
		{
			return GetPool(pool)?.Instantiate<T>(position, rotation, parent);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static Component Instantiate(Component prefab)
		{
			return GetOrCreatePool(prefab).Instantiate();
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static Component Instantiate(Component prefab, Transform parent, bool worldSpace = false)
		{
			return GetOrCreatePool(prefab).Instantiate(parent, worldSpace);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static Component Instantiate(Component prefab, Vector3 position)
		{
			return GetOrCreatePool(prefab).Instantiate(position);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static Component Instantiate(Component prefab, Vector3 position, Quaternion rotation)
		{
			return GetOrCreatePool(prefab).Instantiate(position, rotation);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static Component Instantiate(Component prefab, Vector3 position, Transform parent)
		{
			return GetOrCreatePool(prefab).Instantiate(position, parent);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static Component Instantiate(Component prefab, Vector3 position, Quaternion rotation, Transform parent)
		{
			return GetOrCreatePool(prefab).Instantiate(position, rotation, parent);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static T Instantiate<T>(T prefab) where T: Component
		{
			return GetOrCreatePool(prefab).Instantiate<T>();
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static T Instantiate<T>(T prefab, Transform parent, bool worldSpace = false) where T: Component
		{
			return GetOrCreatePool(prefab).Instantiate<T>(parent, worldSpace);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static T Instantiate<T>(T prefab, Vector3 position) where T: Component
		{
			return GetOrCreatePool(prefab).Instantiate<T>(position);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation) where T: Component
		{
			return GetOrCreatePool(prefab).Instantiate<T>(position, rotation);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static T Instantiate<T>(T prefab, Vector3 position, Transform parent) where T: Component
		{
			return GetOrCreatePool(prefab).Instantiate<T>(position, parent);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T: Component
		{
			return GetOrCreatePool(prefab).Instantiate<T>(position, rotation, parent);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static T Instantiate<T>(string group, T prefab) where T: Component
		{
			return GetOrCreatePool(group, prefab).Instantiate<T>();
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static T Instantiate<T>(string group, T prefab, Transform parent, bool worldSpace = false) where T: Component
		{
			return GetOrCreatePool(group, prefab).Instantiate<T>(parent, worldSpace);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static T Instantiate<T>(string group, T prefab, Vector3 position) where T: Component
		{
			return GetOrCreatePool(group, prefab).Instantiate<T>(position);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static T Instantiate<T>(string group, T prefab, Vector3 position, Quaternion rotation) where T: Component
		{
			return GetOrCreatePool(group, prefab).Instantiate<T>(position, rotation);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static T Instantiate<T>(string group, T prefab, Vector3 position, Transform parent) where T: Component
		{
			return GetOrCreatePool(group, prefab).Instantiate<T>(position, parent);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static T Instantiate<T>(string group, T prefab, Vector3 position, Quaternion rotation, Transform parent) where T: Component
		{
			return GetOrCreatePool(group, prefab).Instantiate<T>(position, rotation, parent);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static T Instantiate<T>(PoolGroup group, T prefab) where T: Component
		{
			return GetOrCreatePool(group, prefab).Instantiate<T>();
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static T Instantiate<T>(PoolGroup group, T prefab, Transform parent, bool worldSpace = false) where T: Component
		{
			return GetOrCreatePool(group, prefab).Instantiate<T>(parent, worldSpace);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static T Instantiate<T>(PoolGroup group, T prefab, Vector3 position) where T: Component
		{
			return GetOrCreatePool(group, prefab).Instantiate<T>(position);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static T Instantiate<T>(PoolGroup group, T prefab, Vector3 position, Quaternion rotation) where T: Component
		{
			return GetOrCreatePool(group, prefab).Instantiate<T>(position, rotation);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static T Instantiate<T>(PoolGroup group, T prefab, Vector3 position, Transform parent) where T: Component
		{
			return GetOrCreatePool(group, prefab).Instantiate<T>(position, parent);
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public static T Instantiate<T>(PoolGroup group, T prefab, Vector3 position, Quaternion rotation, Transform parent)
			where T: Component
		{
			return GetOrCreatePool(group, prefab).Instantiate<T>(position, rotation, parent);
		}

		/// <summary>Pool an instance.</summary>
		public static bool Destroy(Component instance)
		{
			return Destroy(instance.gameObject);
		}

		/// <summary>Pool an instance.</summary>
		public static bool Destroy(GameObject instance)
		{
			if (poolsByGameObject.TryGetValue(instance, out PoolInstanceInfo info))
				info.Pool.Destroy(info.Component);

			return true;
		}

		/// <summary>Pool all instances of a prefab.</summary>
		public static bool DestroyAll(Component prefab)
		{
			Pool pool = GetPool(prefab);
			if (pool == null)
				return false;
			pool.DestroyAll();
			return true;
		}

		/// <summary>Pool all instances of a prefab.</summary>
		public static bool DestroyAll(string name)
		{
			Pool pool = GetPool(name);
			if (pool == null)
				return false;
			pool.DestroyAll();
			return true;
		}

		/// <summary>Pool all instances in a group.</summary>
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
}