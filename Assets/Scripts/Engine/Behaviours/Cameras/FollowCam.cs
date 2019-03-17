using UnityEngine;

namespace Engine.Cameras
{
	[RequireComponent(typeof(Camera))]
	public class FollowCam : MonoBehaviour
	{
		public Transform Target;
		public float Distance = 10.0f;
		public float Speed = 10.0f;

		protected Transform transformCached;

		protected void Awake()
		{
			transformCached = GetComponent<Transform>();
		}

		public void Follow()
		{
			enabled = true;
		}

		public void Follow(Transform target)
		{
			if (target == null)
				return;

			Follow(target, (target.position - transform.position).magnitude);
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
			Vector3 position = target - transformCached.forward * Distance;
			transformCached.position = Vector3.Lerp(transformCached.position, position, Time.deltaTime * Speed);
		}
	}
}