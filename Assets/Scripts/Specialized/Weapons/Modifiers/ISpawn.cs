using System.Collections.Generic;
using UnityEngine;

namespace Weapons.Modifiers
{
	public interface ISpawn
	{
		IEnumerable<Transformation> GetPositions(Vector3 startPosition, Quaternion startRotation);
	}
}