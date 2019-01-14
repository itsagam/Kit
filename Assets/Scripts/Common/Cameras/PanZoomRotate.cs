using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TouchScript;
using TouchScript.Pointers;
using TouchScript.Gestures;

//TODO: Pinch Zoom is lazy at corners in perpective mode
//TODO: Proper rotation clamping
[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(MetaGesture))]
public class PanZoomRotate : MonoBehaviour
{
	public bool Pan = true;
	public bool Zoom = true;
	public bool Rotate = false;
	[Tooltip("Area/bounds to focus on; can be a transform, a renderer or a collider.")]
	public Component View;
	public float PanSpeed = 5.0f;
	[Tooltip("Order of magnitude zoom level affects panning speed; has to be greater than 1 to be applicable.")]
	public float PanZoomFactor = 1.5f;
	public float ZoomSpeed = 0.025f;
	[Tooltip("Minimum orthographicSize if camera is orthographic, camera position in forward axis otherwise.")]
	public float ZoomMin = 1;
	[Tooltip("Maximum orthographicSize if camera is orthographic, camera position in forward axis otherwise.")]
	public float ZoomMax = 10;
	public float RotateSpeed = 0.5f;
	[Tooltip("Smoothing to apply while Lerp-ing")]
	public float Smoothing = 10f;

	protected Camera cameraCached;
	protected Transform transformCached;
	protected MetaGesture gesture;

	protected Vector3 targetPosition;
	protected Quaternion targetRotation;
	protected float targetZoom;
	protected Bounds bounds;
	protected Vector3 forward;
	protected Vector3 forwardAbs;
	protected int forwardSign;

	#region Initialization
	protected void Awake()
	{
		transformCached = GetComponent<Transform>();
		cameraCached = GetComponent<Camera>();
		gesture = GetComponent<MetaGesture>();

		forward = transformCached.forward;
		forwardAbs = forward.Abs();
		forwardSign = (int) Mathf.Sign(Vector3.Dot(Vector3.one, forward));

		if (View is Transform t)
			bounds = t.GetBounds();
		else if (View is Renderer r)
			bounds = r.bounds;
		else if (View is Collider c)
			bounds = c.bounds;
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
		targetPosition = transformCached.position;
		targetRotation = transformCached.rotation;
		if (cameraCached.orthographic)
			targetZoom = cameraCached.orthographicSize;
		else
			targetZoom = GetForwardComponent(targetPosition);
		Clamp();
	}
	#endregion

	#region Pinch/Zoom/Rotate
	protected void OnMoved(object sender, EventArgs e)
	{
		if (gesture.ActivePointers.Count == 1)
		{
			if (Pan)
				FlickPan(gesture.NormalizedScreenPosition, gesture.PreviousNormalizedScreenPosition);
		}
		else if (gesture.ActivePointers.Count == 2)
		{
			Pointer touch1 = gesture.ActivePointers[0];
			Pointer touch2 = gesture.ActivePointers[1];

			if (Zoom)
				PinchZoom(touch1.Position, touch2.Position, touch1.PreviousPosition, touch2.PreviousPosition);

			if (Rotate)
				TwistRotate(touch1.Position, touch2.Position, touch1.PreviousPosition, touch2.PreviousPosition);
		}
		Clamp();
	}

	protected void FlickPan(Vector2 position, Vector2 previousPosition)
	{
		Vector3 delta = previousPosition - position;

		// Compute only if PanZoomFactor is provided/valid
		float zoomFactor = PanZoomFactor > 1 ? GetZoomMapped(1, PanZoomFactor) : 1;

		// Multiplying by zoomFactor so that panning is faster at higher zoom levels
		targetPosition += transformCached.TransformDirection(delta * PanSpeed * zoomFactor);
	}

	protected void PinchZoom(Vector2 position1, Vector2 position2, Vector2 previousPosition1, Vector2 previousPosition2)
	{
		Vector2 viewSize = cameraCached.pixelRect.size;
		float previousDeltaMagnitude = (previousPosition1 - previousPosition2).magnitude;
		float deltaMagnitude = (position1 - position2).magnitude;
		float deltaMagnitudeDifference = previousDeltaMagnitude - deltaMagnitude;

		float sign = 1;
		if (!cameraCached.orthographic)
			// In ortho increasing orthographicSize always zoomes out, in perpective mode it depends on forward
			// If forward is positive increasing forward component zoomes in, if it's negative increasing forward component zoomes out 
			sign = -forwardSign;

		float delta = deltaMagnitudeDifference * ZoomSpeed * sign;

		float newTargetZoom = targetZoom + delta;
		// If new zoom level is out of bounds, continuing to zoom pans the camera – this prevents that
		if (!MathHelper.IsInRange(newTargetZoom, ZoomMin, ZoomMax))
			return;
		targetZoom = newTargetZoom;

		float frustumHeight = GetFrustumHeight();
		targetPosition = targetPosition
			+ transformCached.TransformDirection((previousPosition1 + previousPosition2 - viewSize) * frustumHeight / viewSize.y) * sign
			- transformCached.TransformDirection((position1 + position2 - viewSize) * (frustumHeight + delta) / viewSize.y) * sign;
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
		if (cameraCached.orthographic)
		{
			return targetZoom;	
		}
		else
		{
			// Will be origin if bounds are not provided and camera will try to focus (0, 0, 0) accordingly
			Vector3 viewPosition = bounds.center;
			Vector3 cameraPosition = SetForwardComponent(targetPosition, targetZoom);
			// Get the distance between the camera and view in forward axis (using absolute value since the difference can be negative)
			float viewDistance = Math.Abs(GetForwardComponent(viewPosition - cameraPosition));
			
			// Calculate frustum height from view distance (https://docs.unity3d.com/Manual/FrustumSizeAtDistance.html)
			float frustumHeight = viewDistance * Mathf.Tan(cameraCached.fieldOfView * 0.5f * Mathf.Deg2Rad);

			return frustumHeight;
		}
	}

	protected float GetZoomMapped(float min, float max)
	{
		// If in orthographic mode or if forward is negative, higher targetZoom means lower zoom, so we invert the output range
		if (cameraCached.orthographic || forwardSign < 0)
			return MathHelper.Map(targetZoom, ZoomMin, ZoomMax, max, min);
		else
			return MathHelper.Map(targetZoom, ZoomMin, ZoomMax, min, max);
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
		if (bounds.extents != Vector3.zero)
		{
			float frustumHeight = GetFrustumHeight();
			Vector2 frustum = new Vector3(frustumHeight * cameraCached.aspect, frustumHeight);
			
			float angle = GetForwardComponent(targetRotation.eulerAngles);
			// Plot a function that returns 0 at 0 degrees, 1 at 90 degrees, and 0 again at 180 degrees (higher values reset the cycle)
			float angleAmount = Mathf.Abs(Mathf.Sin(angle * Mathf.Deg2Rad));
			// Swap width and height of the frustum depending on angle (using this instead of "targetRotation * frustum" because that doesn't work on width/height)
			frustum = new Vector2(Mathf.Lerp(frustum.x, frustum.y, angleAmount), Mathf.Lerp(frustum.y, frustum.x, angleAmount));
			
			// Clamp camera position to its bounds, equal to view bounds shrunk by frustum size
			Vector3 clamped = targetPosition.Clamp(bounds.min + frustum.ToVector3(), bounds.max - frustum.ToVector3());

			// Set the clamped vector, but use the original forward component
			targetPosition = SetForwardComponent(clamped, GetForwardComponentVector(targetPosition));
		}
	}

	protected void LateUpdate()
	{
		float fraction = Smoothing * Time.deltaTime;
		if (cameraCached.orthographic)
			cameraCached.orthographicSize = Mathf.Lerp(cameraCached.orthographicSize, targetZoom, fraction);
		else
			targetPosition = SetForwardComponent(targetPosition, targetZoom);

		transformCached.position = Vector3.Lerp(transformCached.position, targetPosition, fraction);
		transformCached.rotation = Quaternion.Slerp(transformCached.rotation, targetRotation, fraction);
	}
	#endregion

}