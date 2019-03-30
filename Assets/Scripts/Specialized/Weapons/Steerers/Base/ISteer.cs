using Unity.Mathematics;

namespace Weapons
{
	public interface ISteer
	{
		float GetPosition(float3 position, quaternion rotation);
		float GetRotation(float3 position, quaternion rotation);
	}
}