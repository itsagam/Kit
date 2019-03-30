using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace Weapons.Steerers
{
	public class Random : ISteer
	{
		[MinValue(0)]
		public float MoveSpeed = 20.0f;
		[MinValue(0)]
		public float RotateSpeed = 5.0f;

		public float3 GetPosition(Transform bullet)
		{
			return MoveSpeed > 0 ?
					   math.mul(bullet.rotation, math.up()) * WeaponSystem.Random.NextFloat(0, MoveSpeed) :
					   float3.zero;
		}

		public quaternion GetRotation(Transform bullet)
		{
			return RotateSpeed > 0 ?
				       quaternion.RotateZ(WeaponSystem.Random.NextFloat(-RotateSpeed, RotateSpeed)) :
					   quaternion.identity;
		}
	}
}