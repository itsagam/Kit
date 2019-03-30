using Unity.Mathematics;
using UnityEngine;

namespace Weapons.Steerers
{
	public class Rotate: ISteer
	{
		public float MoveSpeed = 20;
		public float RotateSpeed = 0.5f;

		public float3 GetPosition(Transform bullet)
		{
			return math.mul(bullet.rotation, math.up()) * MoveSpeed;
		}

		public quaternion GetRotation(Transform bullet)
		{
			return quaternion.RotateZ(RotateSpeed);
		}
	}
}