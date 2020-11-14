using UnityEngine;

namespace Engine.Behaviours
{
	/// <summary>
	/// Orbits the <see cref="Camera"/> (or <see cref="Transform"/>) around a given <see cref="Transform"/>.
	/// </summary>
	public class OrbitTransform: MonoBehaviour
	{
		/// <summary>
		/// Transform to orbit around.
		/// </summary>
		[Tooltip("Transform to orbit around.")]
		public Transform Target;

		/// <summary>
		/// The axis to use while orbiting.
		/// </summary>
		[Tooltip("The axis to use while orbiting.")]
		public Vector3 Axis = Vector3.up;

		/// <summary>
		/// The speed at which to orbit.
		/// </summary>
		[Tooltip("The speed at which to orbit.")]
		public float Speed = 10.0f;

		protected new Transform transform;

		protected void Awake()
		{
			transform = base.transform;
		}

		protected void LateUpdate()
		{
			transform.RotateAround(Target.position, Axis, Time.deltaTime * Speed);
		}
	}
}