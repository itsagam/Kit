using System.Collections.Generic;
using UnityEngine;

namespace Weapons.Modifiers.Spawners
{
	public class Arc : Circle
	{
		[Range(0, 360)]
		public float Angle = 60.0f;

		public override IEnumerable<Transformation> GetPositions(Vector3 startPosition, Quaternion startRotation)
		{
			float angleDiff = Angle / (Count - 1);
			float rotation = (180 - Angle) / 2;
			return GetPositions(startPosition, startRotation, angleDiff, rotation);
		}
	}
}