using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TouchScript;
using TouchScript.Pointers;
using TouchScript.Gestures;

//TODO: Frustum doesn't work at different targetZoom in perpective mode
//TODO: Check and make Rotation work
[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(MetaGesture))]
public class PanZoomRotate : MonoBehaviour
{
	public bool Pan = true;
	public bool Zoom = true;
	public bool Rotate = true;
	public float PanSpeed = 5.0f;
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

		// Adding 1 so that panning is possible when fully zoomed out (zoom level = 0), the new range is 1...2
		float zoomLevel = 1 + GetZoomLevel();
	
		// Multiplying by zoomLevel so that panning is faster at higher zoom levels
		targetPosition += transformCached.TransformDirection(delta * zoomLevel * PanSpeed);
	}

	protected void PinchZoom(Vector2 position1, Vector2 position2, Vector2 previousPosition1, Vector2 previousPosition2)
	{
		Vector2 viewSize = cameraCached.pixelRect.size;
		float previousDeltaMagnitude = (previousPosition1 - previousPosition2).magnitude;
		float deltaMagnitude = (position1 - position2).magnitude;
		float deltaMagnitudeDifference = previousDeltaMagnitude - deltaMagnitude;

		float sign = 1;
		float multiplier = 1;
		if (!cameraCached.orthographic)
		{
			// In ortho increasing orthographicSize always zoomes out, in perpective mode it depends on forward
			// If forward is positive increasing forward component zoomes in, if it's negative increasing forward component zoomes out 
			sign = -forwardSign;
			// Zoom speed is twice as fast in perspective mode
			multiplier = 0.5f;
		}

		float newTargetZoom = targetZoom + (deltaMagnitudeDifference * ZoomSpeed) * sign;

		// If new zoom level is out of bounds, continuing to zoom pans the camera – this prevents that
		if (!MathHelper.IsInRange(newTargetZoom, ZoomMin, ZoomMax))
			return;

		targetPosition += transformCached.TransformDirection((previousPosition1 + previousPosition2 - viewSize) * targetZoom / viewSize.y) * sign * multiplier;
		targetZoom = newTargetZoom;
		targetPosition -= transformCached.TransformDirection((position1 + position2 - viewSize) * targetZoom / viewSize.y) * sign * multiplier;
	}

	protected void TwistRotate(Vector2 position1, Vector2 position2, Vector2 previousPosition1, Vector2 previousPosition2)
	{
		float angle = MathHelper.AngleBetween(position1, position2);
		float previousAngle = MathHelper.AngleBetween(previousPosition1, previousPosition2);
		float deltaAngle = Mathf.DeltaAngle(angle, previousAngle);
		targetRotation = Quaternion.AngleAxis(deltaAngle * RotateSpeed, forward) * targetRotation;
	}

	protected float GetZoomLevel()
	{
		// InverseLerp normalizes a value, so here we get targetZoom normalized to 0...1
		float zoomLevel = Mathf.InverseLerp(ZoomMin, ZoomMax, targetZoom);

		// If in orthographic mode or if forward is negative, higher targetZoom means lower zoom, so we invert the result
		if (cameraCached.orthographic || forwardSign < 0)
			zoomLevel = 1 - zoomLevel;

		return zoomLevel;
	}
	#endregion

	#region Update
	protected void Clamp()
	{
		targetZoom = Mathf.Clamp(targetZoom, ZoomMin, ZoomMax);	
		if (PanBounds != null)
		{
			float viewDistance = targetZoom;
			if (!cameraCached.orthographic)
			{
				// viewDistance is higher at lower zoom, so we invert
				float zoomLevel = 1 - GetZoomLevel();
				// Scale range ZoomMin...ZoomMax (which can be negative) to a maximum viewable area
				float viewMax = Mathf.Abs(ZoomMax - ZoomMin);
				// Current viewable area = zoom level * maximum viewable area
				viewDistance = zoomLevel * viewMax;
				// Some padding is required
				viewDistance += 1.0f;
				// Take fieldOfView into acount
				viewDistance *= Mathf.Tan(cameraCached.fieldOfView * 0.5f * Mathf.Deg2Rad);
			}

			Vector2 frustum = new Vector2(viewDistance * cameraCached.aspect, viewDistance);
			//float angle = Vector3.Dot(Vector3.one, Vector3.Project(targetRotation.eulerAngles, forward));
			//frustum = MathHelper.Rotate(frustum, angle);
			//frustum *= 1 - (0.5f * Mathf.Sin(angle % 91 * 2 * Mathf.Deg2Rad));
			frustum = frustum.Abs();

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