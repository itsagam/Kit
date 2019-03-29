using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Weapons.Spawners
{
	public class Arc : Circle
	{
		[Range(0, 360)]
		public float Angle = 60.0f;

		public override IEnumerable<Location> GetLocations(float3 startPosition, quaternion startRotation)
		{
			float angleDiff = Angle / (Count - 1);
			float angleOffset = (180 - Angle) / 2;
			return GetPositions(startPosition, startRotation, angleDiff, angleOffset);
		}
	}
}