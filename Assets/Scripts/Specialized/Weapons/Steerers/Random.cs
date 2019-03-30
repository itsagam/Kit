using Sirenix.OdinInspector;
using Unity.Mathematics;

namespace Weapons.Steerers
{
	public class Random : ISteer
	{
		[MinValue(0)]
		public float MoveSpeed = 20.0f;
		[MinValue(0)]
		public float RotateSpeed = 0.25f;

		public float GetPosition(float3 position, quaternion rotation)
		{
			return MoveSpeed > 0 ? WeaponSystem.Random.NextFloat(0, MoveSpeed) : 0;
		}

		public float GetRotation(float3 position, quaternion rotation)
		{
			return RotateSpeed > 0 ? WeaponSystem.Random.NextFloat(-RotateSpeed, RotateSpeed) : 0;
		}
	}
}