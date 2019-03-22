using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Weapons.Modifiers.Spawners
{
	public class Random : ISpawn
	{
		public Vector2 PositionX = new Vector2(0.0f, 0.0f);
		public Vector2 PositionY = new Vector2(0.0f, 0.0f);
		[MinValue(-180), MaxValue(180)]
		public Vector2 Rotation = new Vector2(-30.0f, 30.0f);

		public IEnumerable<Transformation> GetPositions(Vector3 startPosition, Quaternion startRotation)
		{
			Transformation transformation = new Transformation(startPosition, startRotation);

			if (PositionX.x != 0 || PositionX.y != 0)
				transformation.Position.x += UnityEngine.Random.Range(PositionX.x, PositionX.y);

			if (PositionY.x != 0 || PositionY.y != 0)
				transformation.Position.y += UnityEngine.Random.Range(PositionY.x, PositionY.y);

			if (Rotation.x != 0 || Rotation.y != 0)
				transformation.Rotation *= Quaternion.Euler(0, 0, UnityEngine.Random.Range(Rotation.x, Rotation.y));

			yield return transformation;
		}
	}
}