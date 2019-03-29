using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Weapons.Steerers
{
	public class Homing : ISteer
	{
		public float MoveSpeed = 20;
		public float RotateSpeed = 5;
		public List<string> Tags = new List<string>();

		public Vector3 GetPosition(Transform bullet)
		{
			return bullet.up * MoveSpeed;
		}

		public Quaternion GetRotation(Transform bullet)
		{
			GameObject target = null;
			foreach (string tag in Tags)
			{
				var targets = GameObject.FindGameObjectsWithTag(tag);
				if (targets.Length <= 0)
					continue;

				Vector3 position = bullet.position;
				target = targets.Aggregate((s1, s2) => (s1.transform.position - position).sqrMagnitude <
													   (s2.transform.position - position).sqrMagnitude ? s1 : s2);
			}

			if (target == null)
				return Quaternion.identity;

			Vector3 direction = target.transform.position - bullet.position;
			float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
			float previous = bullet.eulerAngles.z;
			return Quaternion.Euler(0, 0, (angle - 90) * RotateSpeed - previous);
		}
	}
}