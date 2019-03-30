using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace Weapons.Steerers
{
	public class Homing : ISteer
	{
		[MinValue(0)]
		public float MoveSpeed = 20;
		[MinValue(0)]
		public float RotateSpeed = 0.01f;
		public List<string> Tags = new List<string>();

		public float GetPosition(float3 position, quaternion rotation)
		{
			return MoveSpeed;
		}

		public float GetRotation(float3 position, quaternion rotation)
		{
			if (RotateSpeed == 0)
				return 0;

			var targets = Tags.SelectMany(GameObject.FindGameObjectsWithTag).Select(go => go.transform).ToList();
			if (targets.Count <= 0)
				return 0;

			Vector3 positionVector3 = position;
			Transform target = targets.Aggregate((t1, t2) => (t1.position - positionVector3).sqrMagnitude <
															 (t2.position - positionVector3).sqrMagnitude ? t1 : t2);
			Vector3 direction = target.position - positionVector3;
			float angle = math.atan2(direction.y, direction.x);
			return (angle - 1.5708f - GetZAngle(rotation)) * RotateSpeed; // In radians, the 1.57 part is 90 degrees in radians as well
		}

		protected static float GetZAngle(quaternion rotation)
		{
			float sinYCosP = 2.0f * (rotation.value.w * rotation.value.z + rotation.value.x * rotation.value.y);
			float cosYCosP = -1.0f * (rotation.value.y * rotation.value.y + rotation.value.z * rotation.value.z);
			return math.atan2(sinYCosP, cosYCosP);
		}
	}
}