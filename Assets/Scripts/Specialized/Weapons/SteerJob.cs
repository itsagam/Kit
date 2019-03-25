using Unity.Burst;
using UnityEngine;
using UnityEngine.Jobs;

namespace Weapons.Modifiers
{
	[BurstCompile]
	public struct SteerJob : IJobParallelForTransform
	{
		public float DeltaTime;

		public void Execute(int index, TransformAccess transform)
		{
			transform.position += Vector3.up * 20.0f * DeltaTime;
		}
	}
}