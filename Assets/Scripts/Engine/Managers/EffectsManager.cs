using Engine.Pooling;
using UniRx;
using UnityEngine;

namespace Engine
{
	public static class EffectsManager
	{
		public const string Group = "Effects";

		public static ParticleSystem Spawn(ParticleSystem prefab, Vector3 position)
		{
			if (prefab == null)
				return null;

			ParticleSystem particleSystem = Pooler.Instantiate(Group, prefab, position);
			QueueForDestroy(particleSystem);
			return particleSystem;
		}

		public static ParticleSystem Spawn(ParticleSystem prefab, Vector3 position, Quaternion rotation)
		{
			if (prefab == null)
				return null;

			ParticleSystem particleSystem = Pooler.Instantiate(Group, prefab, position, rotation);
			QueueForDestroy(particleSystem);
			return particleSystem;
		}

		public static ParticleSystem Spawn(ParticleSystem prefab, Transform parent, bool worldSpace = false)
		{
			if (prefab == null)
				return null;

			ParticleSystem particleSystem = Pooler.Instantiate(Group, prefab, parent, worldSpace);
			QueueForDestroy(particleSystem);
			return particleSystem;
		}

		public static ParticleSystem Spawn(ParticleSystem prefab, Transform parent, Vector3 position)
		{
			if (prefab == null)
				return null;

			ParticleSystem particleSystem = Pooler.Instantiate(Group, prefab);
			Transform transform = particleSystem.transform;
			transform.parent = parent;
			transform.localPosition = position;
			QueueForDestroy(particleSystem);
			return particleSystem;
		}

		public static ParticleSystem Spawn(ParticleSystem prefab, Transform parent, Vector3 position, Quaternion rotation)
		{
			if (prefab == null)
				return null;

			ParticleSystem particleSystem = Pooler.Instantiate(Group, prefab);
			Transform transform = particleSystem.transform;
			transform.parent = parent;
			transform.localPosition = position;
			transform.localRotation = rotation;
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
			return Pooler.DestroyAllInGroup(Group);
		}

		private static void QueueForDestroy(ParticleSystem system)
		{
			if (system.main.loop)
				return;

			Observable.EveryUpdate().First(l => !system.IsAlive(true)).CatchIgnore()
					  .Subscribe(l => Pooler.Destroy(system));
		}
	}
}
