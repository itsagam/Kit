using System;
using Sirenix.OdinInspector;
using TouchScript.Gestures;
using TouchScript.Pointers;
using UnityEngine;

namespace Engine.Behaviours
{
	/// <summary>
	/// Provides a robust gesture-oriented Pan/Zoom/Rotate functionality for a camera. Supports both orthographic and perspective modes.
	/// </summary>
	[RequireComponent(typeof(Camera))]
	[RequireComponent(typeof(MetaGesture))]
	public class PanZoomRotateCam : MonoBehaviour
	{
		#region Fields
		/// <summary>
		/// Area/bounds to focus on, be it a transform, renderer or collider.
		/// </summary>
		/// <remarks>Will be origin if not provided.</remarks>
		[Tooltip("Area/bounds to focus on, be it a transform, renderer or collider; will be origin if not provided.")]
		[SceneObjectsOnly]
		public Component View;

		/// <summary>
		/// Smoothing to apply while Lerp-ing.
		/// </summary>
		/// <remarks>Applies to position, rotation and orthographicSize.</remarks>
		[Tooltip("Smoothing to apply while Lerp-ing. Applies to position, rotation and orthographicSize.")]
		[MinValue(0.0f)]
		public float Smoothing = 10f;


		/// <summary>
		/// Whether to allow panning.
		/// </summary>
		[ToggleGroup("Pan")]
		public bool Pan = true;

		/// <summary>
		/// Panning speed.
		/// </summary>
		/// <remarks>Depends on world scale.</remarks>
		[ToggleGroup("Pan")]
		[Tooltip("Panning speed. Depends on world scale.")]
		[MinValue(0.0f)]
		public float PanSpeed = 5.0f;

		/// <summary>
		/// Order of magnitude zoom level affects panning speed.
		/// </summary>
		[ToggleGroup("Pan")]
		[Tooltip("Order of magnitude zoom level affects panning speed.")]
		[MinValue(1.0f)]
		public float PanZoomFactor = 1.5f;


		/// <summary>
		/// Whether to allow zooming.
		/// </summary>
		[ToggleGroup("Zoom")]
		public bool Zoom = true;

		/// <summary>
		/// Zooming speed.
		/// </summary>
		/// <remarks>Depends on world scale in perspective mode.</remarks>
		[ToggleGroup("Zoom")]
		[Tooltip("Zooming speed. Depends on world scale in perspective mode.")]
		[MinValue(0.0f)]
		public float ZoomSpeed = 0.025f;

		/// <summary>
		/// Minimum orthographicSize if camera is orthographic, camera position in forward axis otherwise.
		/// </summary>
		[ToggleGroup("Zoom")]
		[Tooltip("Minimum orthographicSize if camera is orthographic, camera position in forward axis otherwise.")]
		public float ZoomMin = 1;

		/// <summary>
		/// Maximum orthographicSize if camera is orthographic, camera position in forward axis otherwise.
		/// </summary>
		[ToggleGroup("Zoom")]
		[Tooltip("Maximum orthographicSize if camera is orthographic, camera position in forward axis otherwise.")]
		public float ZoomMax = 10;


		/// <summary>
		/// Whether to allow rotation.
		/// </summary>
		[ToggleGroup("Rotate")]
		public bool Rotate = false;

		/// <summary>
		/// The rotation speed.
		/// </summary>
		[ToggleGroup("Rotate")]
		[Tooltip("Rotation speed.")]
		[MinValue(0.0f)]
		public float RotateSpeed = 0.5f;

		protected new Camera camera;
		protected new Transform transform;
		protected MetaGesture gesture;
		protected Bounds bounds;

		protected Vector3 forward;
		protected Vector3 forwardAbs;
		protected int forwardSign;
		protected Quaternion cameraRotation;
		protected Quaternion cameraRotationInv;

		protected Vector3 targetPosition;
		protected Quaternion targetRotation;
		protected float targetZoom;
		#endregion

		#region Initialization
		protected void Awake()
		{
			transform = base.transform;
			camera = GetComponent<Camera>();
			gesture = GetComponent<MetaGesture>();
			bounds = View.GetBounds();
			forward = transform.forward;
			forwardAbs = forward.Abs();
			forwardSign = (int) Mathf.Sign(Vector3.Dot(Vector3.one, forward));
			cameraRotation = SetForwardComponent(transform.rotation.eulerAngles, 0).ToQuaternion();
			cameraRotationInv = Quaternion.Inverse(cameraRotation);
		}

		protected void OnEnable()
		{
			gesture.PointerUpdated += OnMoved;
			Refresh();
		}

		protected void OnDisable()
		{
			gesture.PointerUpdated -= OnMoved;
		}

		public void Refresh()
		{
			targetPosition = transform.position;
			targetRotation = transform.rotation;
			targetZoom = camera.orthographic ? camera.orthographicSize : GetForwardComponent(targetPosition);
			Clamp();
		}
		#endregion

		#region Pinch/Zoom/Rotate
		protected void OnMoved(object sender, EventArgs e)
		{
			switch (gesture.ActivePointers.Count)
			{
				case 1:
				{
					if (Pan)
						FlickPan(gesture.NormalizedScreenPosition, gesture.PreviousNormalizedScreenPosition);
					break;
				}
				case 2:
				{
					Pointer touch1 = gesture.ActivePointers[0];
					Pointer touch2 = gesture.ActivePointers[1];

					if (Zoom)
						PinchZoom(touch1.Position, touch2.Position, touch1.PreviousPosition, touch2.PreviousPosition);

					if (Rotate)
						TwistRotate(touch1.Position, touch2.Position, touch1.PreviousPosition, touch2.PreviousPosition);
					break;
				}
			}

			Clamp();
		}

		protected void FlickPan(Vector2 position, Vector2 previousPosition)
		{
			Vector3 delta = previousPosition - position;

			// Compute only if PanZoomFactor is provided/valid
			float zoomFactor = PanZoomFactor > 1 ? GetZoomMapped(1, PanZoomFactor) : 1;

			// Multiplying by zoomFactor so that panning is faster at higher zoom levels
			targetPosition += transform.TransformDirection(delta * PanSpeed * zoomFactor);
		}

		protected void PinchZoom(Vector2 position1, Vector2 position2, Vector2 previousPosition1, Vector2 previousPosition2)
		{
			Vector2 viewSize = camera.pixelRect.size;
			float previousDeltaMagnitude = (previousPosition1 - previousPosition2).magnitude;
			float deltaMagnitude = (position1                 - position2).magnitude;
			float deltaMagnitudeDifference = previousDeltaMagnitude - deltaMagnitude;

			float direction = 1;
			float magnitude = 1;
			if (!camera.orthographic)
			{
				// In ortho increasing orthographicSize always zooms out, in perspective mode it depends on forward
				// If forward is positive increasing forward component zooms in, if it's negative increasing forward component zooms out
				direction = -forwardSign;
				// Zoom effect is twice as faster by default in perspective mode
				magnitude = 0.5f;
			}

			float delta = deltaMagnitudeDifference * ZoomSpeed * direction;

			float newTargetZoom = targetZoom + delta;
			// If new zoom level is out of bounds, continuing to zoom pans the camera – this prevents that
			if (!MathHelper.IsInRange(newTargetZoom, ZoomMin, ZoomMax))
				return;
			targetZoom = newTargetZoom;

			float frustumHeight = GetFrustumHeight();
			targetPosition = targetPosition
						   + transform.TransformDirection((previousPosition1 + previousPosition2 - viewSize) * frustumHeight / viewSize.y) * direction * magnitude
						   - transform.TransformDirection((position1 + position2 - viewSize) * (frustumHeight + delta) / viewSize.y) * direction * magnitude;
		}

		protected void TwistRotate(Vector2 position1, Vector2 position2, Vector2 previousPosition1, Vector2 previousPosition2)
		{
			float angle = MathHelper.AngleBetween(position1, position2);
			float previousAngle = MathHelper.AngleBetween(previousPosition1, previousPosition2);
			float deltaAngle = Mathf.DeltaAngle(angle, previousAngle);
			targetRotation = Quaternion.AngleAxis(deltaAngle * RotateSpeed, forward) * targetRotation;
		}
		#endregion

		#region Calculations
		protected float GetFrustumHeight()
		{
			if (camera.orthographic)
				return targetZoom;

			// Will be origin if bounds are not provided and camera will try to focus (0, 0, 0) accordingly
			Vector3 viewPosition = bounds.center;
			Vector3 cameraPosition = SetForwardComponent(targetPosition, targetZoom);
			// Get the distance between the camera and view in forward axis (using absolute value since the difference can be negative)
			float viewDistance = Math.Abs(GetForwardComponent(viewPosition - cameraPosition));

			// Calculate frustum height from view distance (https://docs.unity3d.com/Manual/FrustumSizeAtDistance.html)
			float frustumHeight = viewDistance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);

			return frustumHeight;
		}

		protected float GetZoomMapped(float min, float max)
		{
			// If in orthographic mode or if forward is negative, higher targetZoom means lower zoom, so we invert the output range
			return camera.orthographic || forwardSign < 0 ?
					   MathHelper.Map(targetZoom, ZoomMin, ZoomMax, max, min) :
					   MathHelper.Map(targetZoom, ZoomMin, ZoomMax, min, max);
		}

		protected float GetForwardComponent(Vector3 vector)
		{
			// Get the forward component value (Dot product multiples each component of two vectors and returns the sum)
			// Using absolute value of forward so that forward component is not inverted if forward is negative (e.g. when camera is facing down)
			return Vector3.Dot(vector, forwardAbs);
		}

		protected Vector3 GetForwardComponentVector(Vector3 vector)
		{
			// Project keeps just the given normal (in our case forward), so we separate the forward component
			return Vector3.Project(vector, forwardAbs);
		}

		protected Vector3 SetForwardComponent(Vector3 vector, Vector3 newForward)
		{
			// ProjectOnPlane excludes given normal (in our case forward), then we set it to newForward by adding it
			return Vector3.ProjectOnPlane(vector, forwardAbs) + newForward;
		}

		protected Vector3 SetForwardComponent(Vector3 vector, float newForward)
		{
			return SetForwardComponent(vector, newForward * forwardAbs);
		}
		#endregion

		#region Update
		protected void Clamp()
		{
			targetZoom = Mathf.Clamp(targetZoom, ZoomMin, ZoomMax);
			// Don't clamp if bounds are not provided
			if (bounds.extents == Vector3.zero)
				return;

			float frustumHeight = GetFrustumHeight();
			Vector2 frustum = new Vector2(frustumHeight * camera.aspect, frustumHeight);

			// Calculate applied rotation by multiplying target rotation with inverse camera space
			Quaternion rotation = targetRotation * cameraRotationInv;
			float angle = GetForwardComponent(rotation.eulerAngles);
			// Plot a function that returns 0 at 0 degrees, 1 at 90 degrees, and 0 again at 180 degrees (higher values reset the cycle)
			float angleAmount = Mathf.Abs(Mathf.Sin(angle * Mathf.Deg2Rad));
			// Swap width and height of the frustum depending on angle (have to use this, not "targetRotation * frustum" because that doesn't work on width/height)
			frustum = new Vector2(Mathf.Lerp(frustum.x, frustum.y, angleAmount), Mathf.Lerp(frustum.y, frustum.x, angleAmount));

			// Convert frustum from 2d space to 3d space using camera space
			Vector3 frustum3D = cameraRotation * frustum;
			// Rotation can result in negative values, invalidating the frustum
			frustum3D = frustum3D.Abs();

			// Clamp camera position to its bounds, equal to view bounds shrunk by frustum size
			Vector3 clamped = targetPosition.Clamp(bounds.min + frustum3D, bounds.max - frustum3D);
			// Set the clamped vector, but use the original forward component
			targetPosition = SetForwardComponent(clamped, GetForwardComponentVector(targetPosition));
		}

		protected void LateUpdate()
		{
			float fraction = Smoothing * Time.deltaTime;
			if (camera.orthographic)
				camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, targetZoom, fraction);
			else
				targetPosition = SetForwardComponent(targetPosition, targetZoom);

			transform.position = Vector3.Lerp(transform.position, targetPosition, fraction);
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, fraction);
		}
		#endregion

	}
}