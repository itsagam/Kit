using System;
using Sirenix.OdinInspector;
using TouchScript.Gestures.TransformGestures;
using UnityEngine;

namespace Engine.Behaviours
{
	/// <summary>
	/// Orbits & zooms in/out a <see cref="Transform"/> around another one with gestures.
	/// </summary>
	[RequireComponent(typeof(ScreenTransformGesture))]
	public class OrbitCam: MonoBehaviour
	{
		/// <summary>
		/// The <see cref="Transform"/> to orbit around.
		/// </summary>
		[Tooltip("The transform to orbit around.")]
		public Transform Target;

		/// <summary>
		/// Should orbit using screen X-axis?
		/// </summary>
		[Title("Orbit")]
		[LabelText("X")]
		[Tooltip("Should orbit using screen X-axis?")]
		public bool OrbitX = true;

		/// <summary>
		/// Should orbit using screen Y-axis?
		/// </summary>
		[LabelText("Y")]
		[Tooltip("Should orbit using screen Y-axis?")]
		public bool OrbitY = true;

		/// <summary>
		/// The speed at which to orbit.
		/// </summary>
		[LabelText("Speed")]
		[Tooltip("The speed at which to orbit.")]
		public float OrbitSpeed = 0.25f;

		/// <summary>
		/// The speed at which to zoom.
		/// </summary>
		[Title("Zoom")]
		[LabelText("Speed")]
		[Tooltip("The speed at which to zoom.")]
		public float ZoomSpeed = 2.5f;

		/// <summary>
		/// Minimum and maximum distances to keep while zooming.
		/// </summary>
		[LabelText("Bounds")]
		[Tooltip("Minimum and maximum distances to keep while zooming.")]
		public Vector2 ZoomBounds = new Vector2(2, 10);

		protected ScreenTransformGesture gesture;
		protected new Transform transform;

		protected void Awake()
		{
			gesture = GetComponent<ScreenTransformGesture>();
			transform = base.transform;
		}

		protected void OnEnable()
		{
			gesture.Transformed += OnTransform;
		}

		protected void OnDisable()
		{
			gesture.Transformed -= OnTransform;
		}

		protected void OnTransform(object sender, EventArgs e)
		{
			if (Target == null)
				return;

			if (Mathf.Approximately(gesture.DeltaScale, 1.0f))
			{
				Vector3 delta = gesture.DeltaPosition * OrbitSpeed;
				if (OrbitX)
					transform.RotateAround(Target.position, transform.up, delta.x);
				if (OrbitY)
					transform.RotateAround(Target.position, transform.right, -delta.y);
			}
			else
			{
				Vector3 delta = transform.forward * (gesture.DeltaScale - 1f) * ZoomSpeed;
				Vector3 newPosition = transform.position + delta;
				float distance = (Target.position - newPosition).magnitude;
				if (distance >= ZoomBounds.x && distance <= ZoomBounds.y)
					transform.position = newPosition;
			}
		}
	}
}