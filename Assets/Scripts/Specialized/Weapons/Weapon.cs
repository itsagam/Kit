#define USE_ENTITY
//#define USE_GAMEOBJECT
//#define USE_JOBS
//#define USE_SPRITE_RENDERER

#if USE_JOBS
using Unity.Jobs;
using UnityEngine.Jobs;
#endif
using System.Collections.Generic;
using Engine;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
#if USE_ENTITY
using Unity.Entities;
using Unity.Transforms;
#else
using System;
using Engine.Pooling;
using UniRx;
using UniRx.Triggers;
#endif

#if USE_SPRITE_RENDERER
using Weapons.Rendering;
#else
using UnityEngine.Rendering;
using Unity.Rendering;
#endif

namespace Weapons
{
	public class Weapon: SerializedMonoBehaviour
	{
		public Sprite Sprite;
		public Mesh Mesh;
		public Material Material;
		public List<ISpawn> Spawners = new List<ISpawn>();
		public List<ISteer> Steerers = new List<ISteer>();
		public List<IImpact> Impacters = new List<IImpact>();

		[FoldoutGroup("Effects")]
		public ParticleSystem FireEffect;
		[FoldoutGroup("Effects")]
		public ParticleSystem SpawnEffect;
		[FoldoutGroup("Effects")]
		public ParticleSystem ImpactEffect;

		protected new Transform transform;

#if USE_GAMEOBJECT
		protected Transform prefab;
		protected List<Transform> fireables = new List<Transform>();
#endif
#if USE_JOBS
		protected Transform prefab;

		protected TransformAccessArray fireables;
		protected SteerJob steerJob;
		protected JobHandle steerJobHandle;
#endif

#if USE_ENTITY
		protected EntityManager entityManager;
		protected Entity prefab;
#endif

		protected void Awake()
		{
			transform = base.transform;
#if USE_JOBS
			fireables = new TransformAccessArray(0);
#endif
#if USE_ENTITY
			entityManager = World.Active.GetOrCreateManager<EntityManager>();
#endif
			prefab = CreatePrefab();
		}

#if USE_ENTITY
		public Entity CreatePrefab()
		{
			Entity entity = entityManager.CreateEntity(ComponentType.ReadOnly<Prefab>(),
													   ComponentType.ReadWrite<LocalToWorld>(),
													   ComponentType.ReadWrite<Translation>(),
			                                           ComponentType.ReadWrite<Rotation>());

			#if USE_SPRITE_RENDERER
			SpriteInstanceRenderer spriteRenderer = new SpriteInstanceRenderer
													{
														Sprite = Sprite.texture,
														PixelsPerUnit = Sprite.pixelsPerUnit,
														Pivot = Sprite.pivot
													};
			entityManager.AddSharedComponentData(entity, spriteRenderer);
			#else
			RenderMesh meshRenderer = new RenderMesh
			 					  {
			 						  mesh = Mesh,
			                          material = Material,
			 						  castShadows = ShadowCastingMode.Off,
			 						  receiveShadows = false
			 					  };
			entityManager.AddSharedComponentData(entity, meshRenderer);
			entityManager.AddComponent(entity, ComponentType.ReadOnly<PerInstanceCullingTag>());
			#endif

			entityManager.AddComponentData(entity, new MoveSpeed { Speed = 20.0f });
			return entity;
		}
#else
		public Transform CreatePrefab()
		{
			GameObject prefabGameObject = new GameObject(name + "Fireable");
			prefabGameObject.SetActive(false);

			Transform prefabTransform = prefabGameObject.transform;
			SpriteRenderer spriteRenderer = prefabGameObject.AddComponent<SpriteRenderer>();
			spriteRenderer.sprite = Sprite;

			Pooler.GetOrCreatePool(prefabTransform).LimitAmount = int.MaxValue;

			return prefabTransform;
		}
#endif

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
			EffectsManager.Spawn(FireEffect, startPosition, startRotation);
		}

#if USE_ENTITY
		public void Spawn(float3 position, quaternion rotation)
		{
			Entity entity = entityManager.Instantiate(prefab);
			entityManager.SetComponentData(entity, new Translation { Value = position });
			entityManager.SetComponentData(entity, new Rotation { Value = rotation });
		}

#else
		public Transform Spawn(Vector3 position, Quaternion rotation)
		{
			Transform instance = Pooler.Instantiate(prefab, position, rotation);
			instance.gameObject.SetActive(true);

			IDisposable subscription = null;
			subscription = instance.OnBecameInvisibleAsObservable()
								   .Subscribe(l =>
											  {
												  Destroy(instance);
												  subscription.Dispose();
											  });

#if USE_JOBS
			fireables.capacity++;
#endif
			fireables.Add(instance);

			EffectsManager.Spawn(SpawnEffect, position, rotation);
			return instance;
		}
#endif

#if USE_GAMEOBJECT || USE_JOBS
		protected void Update()
		{
			Steer();
		}
#endif

#if USE_GAMEOBJECT
		protected void Steer()
		{
			foreach (Transform fireable in fireables)
				Steer(fireable);
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
#endif

#if USE_JOBS
		protected void Steer()
		{
			if (!fireables.isCreated)
				return;

			steerJob = new SteerJob { DeltaTime = Time.deltaTime };
			steerJobHandle = steerJob.Schedule(fireables);
		}

		protected void LateUpdate()
		{
			steerJobHandle.Complete();
		}
#endif

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

#if USE_GAMEOBJECT
		public void Destroy(Transform fireable)
		{
			fireables.Remove(fireable);
			Pooler.Destroy(fireable);
		}
#endif

#if USE_JOBS
		public void Destroy(Transform fireable)
		{
			if (! fireables.isCreated)
				return;

			for (int i = 0; i < fireables.length; i++)
				if (fireables[i] == fireable)
				{
					fireables.RemoveAtSwapBack(i);
					break;
				}

			Pooler.Destroy(fireable);
		}
#endif

#if USE_JOBS
		protected void OnDestroy()
		{
			fireables.Dispose();
		}
#endif
	}
}