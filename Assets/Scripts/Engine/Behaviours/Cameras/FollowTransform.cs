using Sirenix.OdinInspector;
using UnityEngine;

namespace Engine.Behaviours
{
	/// <summary>
	/// Follows a transform while keeping a certain distance.
	/// </summary>
	public class FollowTransform : MonoBehaviour
	{
		/// <summary>
		/// The target transform to follow.
		/// </summary>
		[Tooltip("The target transform to follow.")]
		public Transform Target;

		/// <summary>
		/// The distance to keep while following.
		/// </summary>
		[Tooltip("The distance to keep while following.")]
		public float Distance = 10.0f;

		/// <summary>
		/// The speed at which to follow.
		/// </summary>
		[Tooltip("The speed at which to follow.")]
		public float MoveSpeed = 10.0f;

		/// <summary>
		/// Should the transform keep facing the target?
		/// </summary>
		[Tooltip("Should the transform keep facing the target?")]
		public bool Face = true;

		/// <summary>
		/// The speed at which to face.
		/// </summary>
		[ShowIf("Face")]
		[Tooltip("The speed at which to face.")]
		public float RotateSpeed = 5.0f;

		protected new Transform transform;

		protected void Awake()
		{
			transform = base.transform;
		}

		/// <summary>
		/// Start following again if stopped.
		/// </summary>
		public void Follow()
		{
			enabled = true;
		}

		/// <summary>
		/// Follow a different transform using the current distance between them.
		/// </summary>
		public void Follow(Transform target)
		{
			if (target == null)
				return;

			Follow(target, (target.position - ((Component) this).transform.position).magnitude);
		}

		/// <summary>
		/// Follow a different transform at a specified distance.
		/// </summary>
		public void Follow(Transform target, float distance)
		{
			Target = target;
			Distance = distance;
			enabled = true;
		}

		/// <summary>
		/// Stop following.
		/// </summary>
		public void Stop()
		{
			enabled = false;
		}

		protected void LateUpdate()
		{
			if (Target == null)
			{
				Stop();
				return;
			}

			Vector3 targetPosition = Target.position;
			Vector3 newPosition = targetPosition - transform.forward * Distance;
			transform.position = Vector3.Lerp(transform.position, newPosition, MoveSpeed * Time.deltaTime);
			if (Face)
			{
				Quaternion newRotation = Quaternion.LookRotation(targetPosition - transform.position);
				transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, RotateSpeed * Time.deltaTime);
			}
		}
	}
}