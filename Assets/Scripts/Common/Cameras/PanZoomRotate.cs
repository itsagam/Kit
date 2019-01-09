using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TouchScript;
using TouchScript.Pointers;
using TouchScript.Gestures;

//TODO: Make Forward-independant
//TODO: Clamp is causing weird problems (variable "extra" keeps increasing camera position in ortho mode, function makes camera go mad in perpective mode)
[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(MetaGesture))]
public class PanZoomRotate : MonoBehaviour
{
	public bool Pan = true;
	public bool Zoom = true;
	public bool Rotate = true;
	public float PanSpeed = 3.0f;
	public Transform PanBounds;
	public float ZoomSpeed = 0.025f;
	public float ZoomMin = 2;
	public float ZoomMax = 15;
	public float RotateSpeed = 0.5f;
	public float Smoothing = 10f;

	protected Camera cameraCached;
	protected Transform transformCached;
	protected MetaGesture gesture;

	protected Bounds bounds;
	protected Vector3 targetPosition;
	protected Quaternion targetRotation;
	protected float targetZoom;

	#region Initialization
	protected void Awake()
	{
		transformCached = GetComponent<Transform>();
		cameraCached = GetComponent<Camera>();
		gesture = GetComponent<MetaGesture>();
	
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
			// Get the transformCached.forward component of position (Dot product multiples each component of two vectors and return the sum)
			targetZoom = Vector3.Dot(targetPosition, transformCached.forward);
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
		// Multiplying by targetZoom so that panning is faster at higher zoom levels
		targetPosition += transformCached.TransformDirection(delta * Mathf.Abs(targetZoom) * PanSpeed);
	}

	protected void PinchZoom(Vector2 position1, Vector2 position2, Vector2 previousPosition1, Vector2 previousPosition2)
	{
		Vector2 viewSize = cameraCached.pixelRect.size;
		float previousDeltaMagnitude = (previousPosition1 - previousPosition2).magnitude;
		float deltaMagnitude = (position1 - position2).magnitude;
		float deltaMagnitudeDifference = previousDeltaMagnitude - deltaMagnitude;

		int sign = 1;
		float multiplier = 1;
		if (!cameraCached.orthographic)
		{
			// In perpective mode, increasing transformCached.forward zoomes in, in ortho increasing orthographicSize zoomes out 
			sign = -1;
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
		targetRotation = Quaternion.AngleAxis(deltaAngle * RotateSpeed, transformCached.forward) * targetRotation;
	}
	#endregion

	#region Update
	protected void Clamp()
	{
		targetZoom = Mathf.Clamp(targetZoom, ZoomMin, ZoomMax);	
		if (PanBounds != null)
		{
			float distance = Mathf.Abs(targetZoom);
			//float angle = Vector3.Dot(Vector3.one, Vector3.Project(targetRotation.eulerAngles, transformCached.forward));
			Vector2 frustum = new Vector2();
			frustum.y = distance;
			if (!cameraCached.orthographic)
				frustum *= Mathf.Tan(cameraCached.fieldOfView * 0.5f * Mathf.Deg2Rad);
			frustum.x = frustum.y * cameraCached.aspect;
			//frustum = MathHelper.Rotate(frustum, angle);
			//frustum *= 1 - (0.5f * Mathf.Sin(angle % 91 * 2 * Mathf.Deg2Rad));
			frustum.x = Mathf.Abs(frustum.x);
			frustum.y = Mathf.Abs(frustum.y);
	
			targetPosition.x = Mathf.Clamp(targetPosition.x, bounds.min.x + frustum.x, bounds.max.x - frustum.x);
			targetPosition.y = Mathf.Clamp(targetPosition.y, bounds.min.y + frustum.y, bounds.max.y - frustum.y);
			//targetPosition.z = Mathf.Clamp(targetPosition.z, bounds.min.z + frustum.y, bounds.max.z - frustum.y);
		}
	}

	protected void LateUpdate()
	{
		float fraction = Smoothing * Time.deltaTime;
		if (cameraCached.orthographic)
			cameraCached.orthographicSize = Mathf.Lerp(cameraCached.orthographicSize, targetZoom, fraction);
		else
			// ProjectOnPlane excludes given normal (in our case transformCached.forward), then we set it to target zoom by adding it
			targetPosition = Vector3.ProjectOnPlane(targetPosition, transformCached.forward) + transformCached.forward * targetZoom;

		transformCached.position = Vector3.Lerp(transformCached.position, targetPosition, fraction);
		transformCached.rotation = Quaternion.Slerp(transformCached.rotation, targetRotation, fraction);
	}
	#endregion

}