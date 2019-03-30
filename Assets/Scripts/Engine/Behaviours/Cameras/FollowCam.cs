using UnityEngine;

namespace Engine.Cameras
{
	[RequireComponent(typeof(Camera))]
	public class FollowCam : MonoBehaviour
	{
		public Transform Target;
		public float Distance = 10.0f;
		public float Speed = 10.0f;

		protected new Transform transform;

		protected void Awake()
		{
			transform = base.transform;
		}

		public void Follow()
		{
			enabled = true;
		}

		public void Follow(Transform target)
		{
			if (target == null)
				return;

			Follow(target, (target.position - ((Component) this).transform.position).magnitude);
		}

		public void Follow(Transform target, float distance)
		{
			Target = target;
			Distance = distance;
			enabled = true;
		}

		public void Stop()
		{
			enabled = false;
		}

		protected void LateUpdate()
		{
			if (Target == null)
				Stop();

			Vector3 target = Target.position;
			Vector3 position = target - transform.forward * Distance;
			transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * Speed);
		}
	}
}