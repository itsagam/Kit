using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Weapons.Steerers
{
	public class Homing : ISteer
	{
		public float MoveSpeed = 20;
		public float RotateSpeed = 5;
		public List<string> Tags = new List<string>();

		public float3 GetPosition(Transform bullet)
		{
			return math.mul(bullet.rotation, math.up()) * MoveSpeed;
		}

		public quaternion GetRotation(Transform bullet)
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
				return quaternion.identity;

			float3 direction = target.transform.position - bullet.position;
			float angle = math.degrees(math.atan2(direction.y, direction.x));
			float previous = bullet.eulerAngles.z;
			return quaternion.RotateZ(math.radians(angle - 90 * RotateSpeed - previous));
		}
	}
}