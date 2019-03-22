using UnityEngine;

namespace Weapons.Modifiers.Steerers
{
	public class Straight: ISteer
	{
		public float Speed = 20;

		public Vector3 GetPosition(Transform bullet)
		{
			return bullet.up * Speed;
		}

		public Quaternion GetRotation(Transform bullet)
		{
			return Quaternion.identity;
		}
	}
}