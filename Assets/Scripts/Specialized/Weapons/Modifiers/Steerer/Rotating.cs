using UnityEngine;

namespace Weapons.Modifiers.Steerers
{
	public class Rotating: ISteer
	{
		public float MoveSpeed = 20;
		[Range(0, 2)]
		public float RotateSpeed = 0.5f;

		public Vector3 GetPosition(Transform bullet)
		{
			return bullet.up * MoveSpeed;
		}

		public Quaternion GetRotation(Transform bullet)
		{
			return Quaternion.Euler(0, 0, RotateSpeed);
		}
	}
}