using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class EffectsManager : MonoBehaviour
{
	public const string GroupName = "Effects";

	public static ParticleSystem Spawn(ParticleSystem prefab, Vector3 position)
	{
		var particleSystem = Pooler.Instantiate(GroupName, prefab, position);
		QueueForDestroy(particleSystem);
		return particleSystem;
	}

	public static ParticleSystem Spawn(ParticleSystem prefab, Vector3 position, Quaternion rotation)
	{
		var particleSystem = Pooler.Instantiate(GroupName, prefab, position, rotation);
		QueueForDestroy(particleSystem);
		return particleSystem;
	}

	public static ParticleSystem Spawn(ParticleSystem prefab, Transform parent, bool worldPositionStays = false)
	{
		var particleSystem = Pooler.Instantiate(GroupName, prefab, parent, worldPositionStays);
		QueueForDestroy(particleSystem);
		return particleSystem;
	}

	public static ParticleSystem Spawn(ParticleSystem prefab, Vector3 position, Transform parent)
	{
		var particleSystem = Pooler.Instantiate(GroupName, prefab, position, parent);
		QueueForDestroy(particleSystem);
		return particleSystem;
	}

	public static ParticleSystem Spawn(ParticleSystem prefab, Vector3 position, Quaternion rotation, Transform parent)
	{
		var particleSystem = Pooler.Instantiate(GroupName, prefab, position, rotation, parent);
		QueueForDestroy(particleSystem);
		return particleSystem;
	}

	public static bool Despawn(Component instance)
	{
		return Pooler.Destroy(instance);
	}

	public static bool DespawnAll(Component prefab)
	{
		return Pooler.DestroyAll(prefab);
	}

	public static bool DespawnAll(string name)
	{
		return Pooler.DestroyAll(name);
	}

	public static bool DespawnAll()
	{
		return Pooler.DestroyAllInGroup(GroupName);
	}

	private static void QueueForDestroy(ParticleSystem system)
	{
		if (system.main.loop)
			return;

		Observable.EveryUpdate().First(l => !system.IsAlive(true)).CatchIgnore()
			.Subscribe(l =>
			{
				Pooler.Destroy(system);
			});
	}
}
