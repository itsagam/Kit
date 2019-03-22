using System;
using System.Collections.Generic;
using Engine.Pooling;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Weapons.Modifiers;

namespace Weapons
{
	public class Weapon: SerializedMonoBehaviour
	{
		public Transform Prefab;
		public List<ISpawn> Spawners = new List<ISpawn>();
		public List<ISteer> Steerers = new List<ISteer>();

		public List<Transform> Fireables { get; } = new List<Transform>();

		protected new Transform transform;

		protected void Awake()
		{
			transform = GetComponent<Transform>();
			Pooler.GetOrCreatePool(Prefab).LimitAmount = int.MaxValue;
		}

		public void Fire(Vector3 startPosition, Quaternion startRotation)
		{
			var positions = new List<Transformation> { new Transformation(startPosition, startRotation) };
			foreach(ISpawn spawner in Spawners)
			{
				int count = positions.Count;
				for (int i=0; i <count; i++)
				{
					positions.AddRange(spawner.GetPositions(positions[0].Position, positions[0].Rotation));
					positions.RemoveAt(0);
				}
			}
			foreach(Transformation transformation in positions)
				Spawn(transformation.Position, transformation.Rotation);
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

		public Transform Spawn(Vector3 position, Quaternion rotation)
		{
			Transform instance = Pooler.Instantiate(Prefab, position, rotation);
			Fireables.Add(instance);
			Observable.Timer(TimeSpan.FromSeconds(10.0f)).Subscribe(l => Destroy(instance));
			return instance;
		}

		public void Destroy(Transform fireable)
		{
			Fireables.Remove(fireable);
			Pooler.Destroy(fireable);
		}
	}
}