using Unity.Mathematics;
using UnityEngine;

namespace Weapons.Steerers
{
	public class Straight: ISteer
	{
		public float Speed = 20;

		public float3 GetPosition(Transform bullet)
		{
			return math.mul(bullet.rotation, math.up()) * Speed;
		}

		public quaternion GetRotation(Transform bullet)
		{
			return quaternion.identity;
		}
	}
}