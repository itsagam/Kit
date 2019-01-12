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
	public bool Rotate = true;
	public float PanSpeed = 5.0f;
	public float PanZoomFactor = 1.5f;
	public Transform PanBounds;
	public float ZoomSpeed = 0.025f;
	public float ZoomMin = 1;
	public float ZoomMax = 10;
	public float RotateSpeed = 0.5f;
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

		if (PanBounds != null)
			bounds = PanBounds.GetBounds();
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

		float zoomFactor = 1;
		// Compute only if PanZoomFactor is provided/valid
		if (PanZoomFactor > 1)
			zoomFactor = GetZoomMapped(1, PanZoomFactor);

		// Multiplying by zoomFactor so that panning is faster at higher zoom levels
		targetPosition += transformCached.TransformDirection(delta * zoomFactor * PanSpeed);
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
		targetPosition += transformCached.TransformDirection((previousPosition1 + previousPosition2 - viewSize) * frustumHeight / viewSize.y) * sign;
		targetPosition -= transformCached.TransformDirection((position1 + position2 - viewSize) * (frustumHeight + delta) / viewSize.y) * sign;
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
		if (PanBounds != null)
		{
			float frustumHeight = GetFrustumHeight();
			Vector2 frustum = new Vector2(frustumHeight * cameraCached.aspect, frustumHeight);
			//float angle = Vector3.Dot(Vector3.one, Vector3.Project(targetRotation.eulerAngles, forward));
			//frustum = MathHelper.Rotate(frustum, angle);
			//frustum *= 1 - (0.5f * Mathf.Sin(angle % 91 * 2 * Mathf.Deg2Rad));
			//frustum = frustum.Abs();

			Vector3 clamped;
			clamped.x = Mathf.Clamp(targetPosition.x, bounds.min.x + frustum.x, bounds.max.x - frustum.x);
			clamped.y = Mathf.Clamp(targetPosition.y, bounds.min.y + frustum.y, bounds.max.y - frustum.y);
			clamped.z = Mathf.Clamp(targetPosition.z, bounds.min.z + frustum.y, bounds.max.z - frustum.y);

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