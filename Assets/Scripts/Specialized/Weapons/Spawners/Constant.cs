using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Weapons.Spawners
{
	public class Constant : ISpawn
	{
		public Vector2 Position = new Vector2(0.0f, 0.0f);
		[Range(-180, 180)]
		public float Rotation = 0.0f;

		public IEnumerable<Location> GetLocations(float3 startPosition, quaternion startRotation)
		{
			yield return new Location(startPosition + new float3(Position.x, Position.y, 0),
									  math.mul(startRotation, quaternion.RotateZ(math.radians(Rotation))));

		}
	}
}