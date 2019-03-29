using Unity.Mathematics;

namespace Weapons
{
	public struct Location
	{
		public float3 Position;
		public quaternion Rotation;

		public Location(float3 position, quaternion rotation)
		{
			Position = position;
			Rotation = rotation;
		}
	}
}
