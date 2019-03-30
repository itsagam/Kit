using Unity.Mathematics;

namespace Weapons.Steerers
{
	public class Straight: ISteer
	{
		public float Speed = 20;

		public float GetPosition(float3 position, quaternion rotation)
		{
			return Speed;
		}

		public float GetRotation(float3 position, quaternion rotation)
		{
			return 0;
		}
	}
}