using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Weapons
{
	[Serializable]
	public struct MoveSpeed : IComponentData
	{
		public float Speed;
	}
	
	public class SteerSystem : JobComponentSystem
	{
		[BurstCompile]
		private struct SteerJobECS : IJobProcessComponentData<Translation, Rotation, MoveSpeed>
		{
			public float DeltaTime;

			public void Execute(ref Translation translation, ref Rotation rotation, ref MoveSpeed moveSpeed)
			{
				translation.Value += math.mul(rotation.Value, math.up()) * moveSpeed.Speed * DeltaTime;
			}
		}

		protected override JobHandle OnUpdate(JobHandle depends)
		{
			SteerJobECS steerJob = new SteerJobECS { DeltaTime = Time.deltaTime };
			JobHandle steerJobHandle = steerJob.Schedule(this, depends);
			return steerJobHandle;
		}
	}
}