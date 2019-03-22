using UnityEngine;

namespace Weapons.Modifiers
{
	public interface ISteer
	{
		Vector3 GetPosition(Transform bullet);
		Quaternion GetRotation(Transform bullet);
	}
}