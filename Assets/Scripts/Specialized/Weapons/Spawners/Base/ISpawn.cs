using System.Collections.Generic;
using Unity.Mathematics;

namespace Weapons
{
	public interface ISpawn
	{
		IEnumerable<Location> GetLocations(float3 startPosition, quaternion startRotation);
	}
}