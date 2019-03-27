using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Weapons
{
	public class SteerJobSystem : JobComponentSystem
	{
		[BurstCompile]
		public struct SteerJobECS : IJobProcessComponentData<Translation, MoveSpeed>
		{
			public float DeltaTime;

			public void Execute(ref Translation translation, ref MoveSpeed moveSpeed)
			{
				translation.Value += math.up() * moveSpeed.Speed * DeltaTime;
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