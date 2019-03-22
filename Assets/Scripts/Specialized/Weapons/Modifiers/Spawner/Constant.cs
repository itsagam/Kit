using System.Collections.Generic;
using UnityEngine;

namespace Weapons.Modifiers.Spawners
{
	public class Constant : ISpawn
	{
		public Vector2 Position = new Vector2(0.0f, 0.0f);
		[Range(-180, 180)]
		public float Rotation = 0.0f;

		public IEnumerable<Location> GetLocations(Vector3 startPosition, Quaternion startRotation)
		{
			yield return new Location(startPosition + (Vector3) Position,
									  startRotation * Quaternion.Euler(0, 0, Rotation));

		}
	}
}