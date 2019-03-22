using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Weapons.Modifiers.Spawners
{
	public class Circle : ISpawn
	{
		[MinValue(0)]
		public int Count = 20;

		[Range(-180, 180)]
		public float Rotation = 0.0f;

		public virtual IEnumerable<Transformation> GetPositions(Vector3 startPosition, Quaternion startRotation)
		{
			float angleDiff = 360.0f / Count;
			return GetPositions(startPosition, startRotation, angleDiff, 90);
		}

		public virtual IEnumerable<Transformation> GetPositions(Vector3 startPosition, Quaternion startRotation, float angleDiff, float rotation)
		{
			for (int i = 0; i < Count; i++)
			{
				float angle = angleDiff * i + rotation + Rotation;
				float radians = angle * Mathf.Deg2Rad;
				yield return new Transformation(startPosition + new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0),
												startRotation * Quaternion.Euler(0, 0, angle - 90));
			}
		}
	}
}