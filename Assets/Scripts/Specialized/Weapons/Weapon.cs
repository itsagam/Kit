using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Pooling;
using Sirenix.OdinInspector;
using UnityEngine;
using Weapons.Modifiers;

namespace Weapons
{
	public class Weapon: SerializedMonoBehaviour
	{
		public const float RaycastDistance = 1.0f;

		public Transform Prefab;
		public List<ISpawn> Spawners = new List<ISpawn>();
		public List<ISteer> Steerers = new List<ISteer>();
		public List<IImpact> Impacters = new List<IImpact>();

		[FoldoutGroup("Effects")]
		public ParticleSystem FireEffect;
		[FoldoutGroup("Effects")]
		public ParticleSystem SpawnEffect;
		[FoldoutGroup("Effects")]
		public ParticleSystem ImpactEffect;

		public List<Transform> Fireables { get; } = new List<Transform>();
		protected new Transform transform;
		protected static readonly ContactFilter2D contactFilter = new ContactFilter2D().NoFilter();

		protected void Awake()
		{
			transform = GetComponent<Transform>();
			Pooler.GetOrCreatePool(Prefab).LimitAmount = int.MaxValue;
		}

		public void Fire()
		{
			Fire(transform.position, transform.rotation);
		}

		public void Fire(Vector3 startPosition, Quaternion startRotation)
		{
			switch (Spawners.Count)
			{
				case 0:
					Spawn(startPosition, startRotation);
					break;

				case 1:
					foreach(Location location in Spawners[0].GetLocations(startPosition, startRotation))
						Spawn(location.Position, location.Rotation);
					break;

				default:
					var locations = new Queue<Location>();
					locations.Enqueue(new Location(startPosition, startRotation));
					foreach(ISpawn spawner in Spawners)
					{
						int count = locations.Count;
						for (int i=0; i <count; i++)
						{
							Location previous = locations.Dequeue();
							foreach (Location location in spawner.GetLocations(previous.Position, previous.Rotation))
								locations.Enqueue(location);
						}
					}
					foreach(Location location in locations)
						Spawn(location.Position, location.Rotation);
					break;
			}
			EffectsManager.Spawn(FireEffect, startPosition, startRotation);
		}

		public Transform Spawn(Vector3 position, Quaternion rotation)
		{
			Transform instance = Pooler.Instantiate(Prefab, position, rotation);
			Fireables.Add(instance);
			//Observable.Timer(TimeSpan.FromSeconds(10.0f)).Subscribe(l => Destroy(instance));
			EffectsManager.Spawn(SpawnEffect, position, rotation);
			return instance;
		}

		protected void Update()
		{
			foreach (Transform fireable in Fireables)
			{
				Steer(fireable);
			}
		}

		protected void Steer(Transform bullet)
		{
			Vector3 position = Vector3.zero;
			Quaternion rotation = Quaternion.identity;
			foreach(ISteer steerer in Steerers)
			{
				position += steerer.GetPosition(bullet);
				rotation *= steerer.GetRotation(bullet);
			}

			bullet.position += position * Time.deltaTime;
			bullet.rotation *= rotation;
		}

		protected void FixedUpdate()
		{
			for (int i=Fireables.Count-1; i>=0; i--)
				CheckForImpact(Fireables[i]);
		}

		protected void CheckForImpact(Transform fireable)
		{
			Vector3 origin = fireable.position;
			Vector3 direction = fireable.up;
			Physics2D.defaultPhysicsScene.Raycast(origin, direction, RaycastDistance, contactFilter);
			RaycastHit2D hit = Physics2D.Raycast(origin, direction, RaycastDistance);
			if (!ReferenceEquals(hit.transform, null))
				Impact(fireable, hit.transform, hit.point, hit.normal);
		}

		protected void Impact(Transform fireable, Transform impact, Vector3 position, Vector2 normal)
		{
			if (Impacters.All(e => e.OnImpact(fireable, impact, position, normal)))
				Destroy(fireable);
			EffectsManager.Spawn(ImpactEffect, position);
		}

		public void Destroy(Transform fireable)
		{
			Fireables.Remove(fireable);
			Pooler.Destroy(fireable);
		}
	}
}