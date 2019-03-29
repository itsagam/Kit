using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Mathematics;

namespace Weapons.Spawners
{
	public class Multiple: ISpawn
	{
		[MinValue(0)]
		public int Count = 5;
		[MinValue(0)]
		public float Space = 2.0f;

		public IEnumerable<Location> GetLocations(float3 startPosition, quaternion startRotation)
		{
			for (int i=0; i <Count; i++)
			{
				float3 current = math.mul(startRotation, new float3(Space * (i - Count / 2), 0, 0));
				yield return new Location(startPosition + current, startRotation);
			}
		}
	}
}