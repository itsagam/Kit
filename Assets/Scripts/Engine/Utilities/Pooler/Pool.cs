using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pool : MonoBehaviour, IEnumerable<Component>
{
	public Component Prefab;

	protected List<Component> instances = new List<Component>();

	protected void Awake()
	{
		Pooler.CachePool(this);
	}

	protected void OnDestroy()
	{
		Pooler.UncachePool(this);
	}

	public Component Spawn()
	{
		Component instance;
		int index = instances.FindIndex(c => c != null);
		if (index >= 0)
		{
			instance = instances[index];
			instance.gameObject.SetActive(true);
			instances.RemoveAt(index);
		}
		else
		{
			instance = Instantiate(Prefab);
			instance.name = name;
			instance.transform.SetParent(transform);	
		}
		if (instance is IPooled pooled)
			pooled.OnSpawned();
		else
			instance.gameObject.BroadcastMessage(Pooler.SpawnedMethod, SendMessageOptions.DontRequireReceiver);
		return instance;
	}

	public T Spawn<T>() where T: Component
	{
		return (T) Spawn();
	}

	public void Despawn(Component instance)
	{
		if (instance is IPooled pooled)
			pooled.OnDespawned();
		else
			instance.gameObject.BroadcastMessage(Pooler.DespawnedMethod, SendMessageOptions.DontRequireReceiver);
		instance.gameObject.SetActive(false);
		instances.Add(instance);
	}

	public IEnumerator<Component> GetEnumerator()
	{
		return instances.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return instances.GetEnumerator();
	}
}