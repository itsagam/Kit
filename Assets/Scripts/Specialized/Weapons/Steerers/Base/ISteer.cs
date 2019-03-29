using UnityEngine;

namespace Weapons
{
	public interface ISteer
	{
		Vector3 GetPosition(Transform bullet);
		Quaternion GetRotation(Transform bullet);
	}
}