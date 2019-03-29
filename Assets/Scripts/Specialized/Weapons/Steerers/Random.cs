using Sirenix.OdinInspector;
using UnityEngine;

namespace Weapons.Steerers
{
	public class Random : ISteer
	{
		[MinValue(0)]
		public float MoveSpeed = 20.0f;
		[Range(0, 20)]
		public float RotateSpeed = 5.0f;

		public Vector3 GetPosition(Transform bullet)
		{
			return MoveSpeed > 0 ? bullet.up * UnityEngine.Random.Range(0, MoveSpeed) : Vector3.zero;
		}

		public Quaternion GetRotation(Transform bullet)
		{
			return RotateSpeed > 0 ? Quaternion.Euler(0, 0, UnityEngine.Random.Range(-RotateSpeed, RotateSpeed)) : Quaternion.identity;
		}
	}
}