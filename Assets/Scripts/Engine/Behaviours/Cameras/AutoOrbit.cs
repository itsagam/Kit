using UnityEngine;

namespace Engine.Cameras
{
	public class AutoOrbit: MonoBehaviour
	{
		public Transform Target;
		public Vector3 Axis = Vector3.up;
		public float Speed = 10.0f;

		protected void LateUpdate()
		{
			transform.RotateAround(Target.position, Axis, Time.deltaTime * Speed);
		}
	}
}