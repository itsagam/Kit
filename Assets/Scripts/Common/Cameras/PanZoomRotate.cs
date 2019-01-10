using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TouchScript;
using TouchScript.Pointers;
using TouchScript.Gestures;

//TODO: Pinch Zoom is lazy at corners in perpective mode
//TODOL Figure out padding
//TODO: Check and make Rotation work
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
			// Get the forward component of position (Dot product multiples each component of two vectors and return the sum)
			// Using absolute value of forward so that forward component is not inverted if forward is negative (e.g. when camera is facing down)
			targetZoom = Vector3.Dot(targetPosition, forwardAbs);
		Clamp();
	}
	#endregion

	#region PinchZoomRotate
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

		float viewDistance = GetViewDistance();

		targetPosition += transformCached.TransformDirection((previousPosition1 + previousPosition2 - viewSize) * viewDistance / viewSize.y) * sign;
		viewDistance += delta;
		targetPosition -= transformCached.TransformDirection((position1 + position2 - viewSize) * viewDistance / viewSize.y) * sign;

		targetZoom = newTargetZoom;
	}

	protected void TwistRotate(Vector2 position1, Vector2 position2, Vector2 previousPosition1, Vector2 previousPosition2)
	{
		float angle = MathHelper.AngleBetween(position1, position2);
		float previousAngle = MathHelper.AngleBetween(previousPosition1, previousPosition2);
		float deltaAngle = Mathf.DeltaAngle(angle, previousAngle);
		targetRotation = Quaternion.AngleAxis(deltaAngle * RotateSpeed, forward) * targetRotation;
	}

	protected float GetViewDistance()
	{
		if (cameraCached.orthographic)
		{
			return targetZoom;
		}
		else
		{
			// Scale range ZoomMin...ZoomMax (which can be negative) to a maximum viewable area
			float viewMax = Mathf.Abs(ZoomMax - ZoomMin);
			// More area is viewable at lower zooms so min/max are inverted
			float viewDistance = GetZoomMapped(viewMax, 0);
			// Some padding is required
			viewDistance += 0.4f;
			//viewDistance += 1;
			// Take fieldOfView into acount
			viewDistance *= Mathf.Tan(cameraCached.fieldOfView * 0.5f * Mathf.Deg2Rad);
			return viewDistance;
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
	#endregion

	#region Update
	protected void Clamp()
	{
		targetZoom = Mathf.Clamp(targetZoom, ZoomMin, ZoomMax);	
		if (PanBounds != null)
		{
			float viewDistance = GetViewDistance();
			Vector2 frustum = new Vector2(viewDistance * cameraCached.aspect, viewDistance);
			//float angle = Vector3.Dot(Vector3.one, Vector3.Project(targetRotation.eulerAngles, forward));
			//frustum = MathHelper.Rotate(frustum, angle);
			//frustum *= 1 - (0.5f * Mathf.Sin(angle % 91 * 2 * Mathf.Deg2Rad));
			//frustum = frustum.Abs();

			Vector3 clamped;
			clamped.x = Mathf.Clamp(targetPosition.x, bounds.min.x + frustum.x, bounds.max.x - frustum.x);
			clamped.y = Mathf.Clamp(targetPosition.y, bounds.min.y + frustum.y, bounds.max.y - frustum.y);
			clamped.z = Mathf.Clamp(targetPosition.z, bounds.min.z + frustum.y, bounds.max.z - frustum.y);
			// ProjectOnPlane excludes given normal (in our case forward), so we discard the forward component from clamped vector
			Vector3 withoutForward = Vector3.ProjectOnPlane(clamped, forwardAbs);
			// Project keeps just the given normal (in our case forward), so we separate the forward component from original vector
			Vector3 justForward = Vector3.Project(targetPosition, forwardAbs);

			// Combine clamped vector without forward and use the original forward
			targetPosition = withoutForward + justForward;
		}
	}

	protected void LateUpdate()
	{
		float fraction = Smoothing * Time.deltaTime;
		if (cameraCached.orthographic)
			cameraCached.orthographicSize = Mathf.Lerp(cameraCached.orthographicSize, targetZoom, fraction);
		else
			// ProjectOnPlane excludes forward, then we set it to target zoom by adding it
			targetPosition = Vector3.ProjectOnPlane(targetPosition, forwardAbs) + targetZoom * forwardAbs;

		transformCached.position = Vector3.Lerp(transformCached.position, targetPosition, fraction);
		transformCached.rotation = Quaternion.Slerp(transformCached.rotation, targetRotation, fraction);
	}
	#endregion

}