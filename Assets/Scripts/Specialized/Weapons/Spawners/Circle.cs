using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace Weapons.Spawners
{
	public class Circle : ISpawn
	{
		[MinValue(0)]
		public int Count = 10;

		[Range(-180, 180)]
		public float Rotation = 0.0f;

		public virtual IEnumerable<Location> GetLocations(float3 startPosition, quaternion startRotation)
		{
			float angleDiff = 360.0f / Count;
			float angleOffset = 90;
			return GetPositions(startPosition, startRotation, angleDiff, angleOffset);
		}

		public IEnumerable<Location> GetPositions(float3 startPosition, quaternion startRotation, float angleDiff, float angleOffset)
		{
			for (int i = 0; i < Count; i++)
			{
				float degrees = angleDiff * i + angleOffset + Rotation;
				float radians = math.radians(degrees);
				float3 position = new float3(math.cos(radians), math.sin(radians), 0);
				quaternion rotation = quaternion.RotateZ(math.radians(degrees - 90));
				yield return new Location(startPosition + position,
										  math.mul(startRotation, rotation));
			}
		}
	}
}