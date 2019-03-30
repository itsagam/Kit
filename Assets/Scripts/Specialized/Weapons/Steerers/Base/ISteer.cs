using Unity.Mathematics;
using UnityEngine;

namespace Weapons
{
	public interface ISteer
	{
		float3 GetPosition(Transform bullet);
		quaternion GetRotation(Transform bullet);
	}
}