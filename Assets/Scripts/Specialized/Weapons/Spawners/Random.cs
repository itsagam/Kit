using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace Weapons.Spawners
{
	public class Random : ISpawn
	{
		public Vector2 PositionX = new Vector2(0.0f, 0.0f);
		public Vector2 PositionY = new Vector2(0.0f, 0.0f);
		[MinValue(-180), MaxValue(180)]
		public Vector2 Rotation = new Vector2(-30.0f, 30.0f);

		public IEnumerable<Location> GetLocations(float3 startPosition, quaternion startRotation)
		{
			Location location = new Location(startPosition, startRotation);

			if (PositionX.x != 0 || PositionX.y != 0)
				location.Position.x += WeaponSystem.Random.NextFloat(PositionX.x, PositionX.y);

			if (PositionY.x != 0 || PositionY.y != 0)
				location.Position.y += WeaponSystem.Random.NextFloat(PositionY.x, PositionY.y);

			if (Rotation.x != 0 || Rotation.y != 0)
			{
				quaternion rotation = quaternion.RotateZ(math.radians(WeaponSystem.Random.NextFloat(Rotation.x, Rotation.y)));
				location.Rotation = math.mul(location.Rotation, rotation);
			}

			yield return location;
		}
	}
}