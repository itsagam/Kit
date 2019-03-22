using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Weapons.Modifiers.Spawners
{
	public class Multiple: ISpawn
	{
		[MinValue(0)]
		public int Count = 5;
		[MinValue(0)]
		public float Space = 1.0f;

		public IEnumerable<Transformation> GetPositions(Vector3 startPosition, Quaternion startRotation)
		{
			for (int i=0; i <Count; i++)
			{
				Vector3 current = startPosition + startRotation * new Vector3(Space * (i - Count / 2), 0, 0);
				yield return new Transformation(current, startRotation);
			}
		}
	}
}