using System.Collections.Generic;
using UnityEngine;

namespace Weapons.Modifiers
{
	public interface ISpawn
	{
		IEnumerable<Location> GetLocations(Vector3 startPosition, Quaternion startRotation);
	}
}