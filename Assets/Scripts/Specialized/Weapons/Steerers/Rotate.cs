using Unity.Mathematics;

namespace Weapons.Steerers
{
	public class Rotate: ISteer
	{
		public float MoveSpeed = 20;
		public float RotateSpeed = 0.01f;

		public float GetPosition(float3 position, quaternion rotation)
		{
			return MoveSpeed;
		}

		public float GetRotation(float3 position, quaternion rotation)
		{
			return RotateSpeed;
		}
	}
}