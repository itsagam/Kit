using System;
using TouchScript.Gestures.TransformGestures;
using UnityEngine;

namespace Engine.Cameras
{
	[RequireComponent(typeof(ScreenTransformGesture))]
	public class Orbit: MonoBehaviour
	{
		public Transform Target;
		public float RotateSpeed = 0.25f;
		public float ZoomSpeed = 2.5f;
		public Vector2 ZoomBounds = new Vector2(2, 10);
		public bool X = true;
		public bool Y = true;

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
				Vector3 delta = gesture.DeltaPosition * RotateSpeed;
				if (X)
					transform.RotateAround(Target.position, transform.up, delta.x);
				if (Y)
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