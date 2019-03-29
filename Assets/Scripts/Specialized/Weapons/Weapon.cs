using System.Collections.Generic;
using Engine;
using Sirenix.OdinInspector;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Weapons.Rendering;

namespace Weapons
{
	public class Weapon: SerializedMonoBehaviour
	{
		public const float RaycastDistance = 1.0f;

		// MonoBehaviour/Jobs
		//public Transform Prefab;
		// ECS
		public GameObject Prefab;
		public List<ISpawn> Spawners = new List<ISpawn>();
		public List<ISteer> Steerers = new List<ISteer>();
		public List<IImpact> Impacters = new List<IImpact>();

		[FoldoutGroup("Effects")]
		public ParticleSystem FireEffect;
		[FoldoutGroup("Effects")]
		public ParticleSystem SpawnEffect;
		[FoldoutGroup("Effects")]
		public ParticleSystem ImpactEffect;

		//public List<Transform> Fireables { get; } = new List<Transform>();

		protected new Transform transform;
		protected static readonly ContactFilter2D contactFilter = new ContactFilter2D().NoFilter();

		// Job-system
		// protected TransformAccessArray fireables;
		// protected SteerJob steerJob;
		// protected JobHandle steerJobHandle;

		// ECS
		protected EntityManager entityManager;
		protected Entity entityPrefab;

		protected void Awake()
		{
			transform = GetComponent<Transform>();
			entityManager = World.Active.GetOrCreateManager<EntityManager>();
			entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(Prefab, World.Active);
			//Pooler.GetOrCreatePool(Prefab).LimitAmount = int.MaxValue;
		}

		public void Fire()
		{
			Fire(transform.position, transform.rotation);
		}

		public void Fire(float3 startPosition, quaternion startRotation)
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
			Allocate();

			EffectsManager.Spawn(FireEffect, startPosition, startRotation);
		}

		// ECS
		public void Spawn(float3 position, quaternion rotation)
		{
			Entity entity = entityManager.Instantiate(entityPrefab);

			Sprite sprite = Prefab.GetComponent<SpriteRenderer>().sprite;
			SpriteInstanceRenderer spriteRenderer = new SpriteInstanceRenderer
											  {
												  Sprite = sprite.texture,
												  PixelsPerUnit = sprite.pixelsPerUnit,
												  Pivot = sprite.pivot
											  };
			entityManager.AddSharedComponentData(entity, spriteRenderer);

			entityManager.SetComponentData(entity, new Translation { Value = position });
			entityManager.SetComponentData(entity, new Rotation { Value = rotation });
		}

		// MonoBehaviour/Job-system
		// public Transform Spawn(Vector3 position, Quaternion rotation)
		// {
		// 	Transform instance = Pooler.Instantiate(Prefab, position, rotation);
		//
		// 	IDisposable subscription = null;
		// 	subscription = instance.OnBecameInvisibleAsObservable()
		// 						   .Subscribe(l =>
		// 									  {
		// 										  Destroy(instance);
		// 										  subscription.Dispose();
		// 									  });
		// 	Fireables.Add(instance);
		//
		// 	EffectsManager.Spawn(SpawnEffect, position, rotation);
		// 	return instance;
		// }

		// MonoBehaviour/ECS
		protected void Allocate()
		{
		}

		// Job-system
		// protected void Allocate()
		// {
		// 	fireables = new TransformAccessArray(Fireables.Count);
		// 	foreach (Transform fireable in Fireables)
		// 		fireables.Add(fireable);
		// }

		// protected void Update()
		// {
		// 	Steer();
		// }

		// // Job-system Steer
		// protected void Steer()
		// {
		// 	if (!fireables.isCreated)
		// 		return;
		//
		// 	steerJob = new SteerJob { DeltaTime = Time.deltaTime };
		// 	steerJobHandle = steerJob.Schedule(fireables);
		// }

		// MonoBehaviour Steer
		// protected void Steer()
		// {
		// 	foreach (Transform fireable in Fireables)
		// 		Steer(fireable);
		// }

		// protected void Steer(Transform bullet)
		// {
		// 	Vector3 position = Vector3.zero;
		// 	Quaternion rotation = Quaternion.identity;
		// 	foreach(ISteer steerer in Steerers)
		// 	{
		// 		position += steerer.GetPosition(bullet);
		// 		rotation *= steerer.GetRotation(bullet);
		// 	}
		//
		// 	bullet.position += position * Time.deltaTime;
		// 	bullet.rotation *= rotation;
		// }

		// protected void LateUpdate()
		// {
		// 	steerJobHandle.Complete();
		// }

		// protected void FixedUpdate()
		// {
		// 	for (int i=Fireables.Count-1; i>=0; i--)
		// 		CheckForImpact(Fireables[i]);
		// }

		// protected void CheckForImpact(Transform fireable)
		// {
		// 	Vector3 origin = fireable.position;
		// 	Vector3 direction = fireable.up;
		// 	Physics2D.defaultPhysicsScene.Raycast(origin, direction, RaycastDistance, contactFilter);
		// 	RaycastHit2D hit = Physics2D.Raycast(origin, direction, RaycastDistance);
		// 	if (!ReferenceEquals(hit.transform, null))
		// 		Impact(fireable, hit.transform, hit.point, hit.normal);
		// }
		//
		// protected void Impact(Transform fireable, Transform impact, Vector3 position, Vector2 normal)
		// {
		// 	if (Impacters.All(e => e.OnImpact(fireable, impact, position, normal)))
		// 		Destroy(fireable);
		// 	EffectsManager.Spawn(ImpactEffect, position);
		// }

		// MonoBehaviour/Job-system
		// public void Destroy(Transform fireable)
		// {
		// 	Fireables.Remove(fireable);
		// 	Pooler.Destroy(fireable);
		// }

		// Job System
		// protected void OnDestroy()
		// {
		// 	fireables.Dispose();
		// }
	}
}