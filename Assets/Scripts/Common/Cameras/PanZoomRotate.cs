using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript;
using TouchScript.Gestures;


//TODO: Fix this
/*
[RequireComponent(typeof(MetaGesture))]
[RequireComponent(typeof(Camera))]
public class PanZoomRotate : MonoBehaviour
{
	public Vector3 Forward = Vector3.forward;
	public float PanSpeed = 3.0f;
	public Transform PanBounds;
	public float ZoomSpeed = 0.025f;
	public Vector2 ZoomBounds = new Vector2(2, 15);
	public float RotateSpeed = 0.5f;
	public float Smoothing = 10f;

	protected MetaGesture gesture;
	protected Camera cam;
	protected Bounds bounds;
	protected Vector3 positionToGo;
	protected Quaternion rotationToGo;
	protected float zoomToGo;

	protected void Awake()
	{
		print(Vector3.ProjectOnPlane(new Vector3(20, 30, 40), new Vector3(0, 0, 1)));
		gesture = GetComponent<MetaGesture>();
		cam = GetComponent<Camera>();
		if (PanBounds != null)
			bounds = PanBounds.GetBounds();
	}

	protected void OnEnable()
	{
		gesture.TouchMoved += OnTouch;
		Refresh();
	}

	protected void OnDisable()
	{
		gesture.TouchMoved -= OnTouch;
	}

	public void Refresh()
	{
		positionToGo = transform.position;
		rotationToGo = transform.rotation;
		if (cam.orthographic)
			zoomToGo = cam.orthographicSize;
		else
			zoomToGo = Vector3.Dot(Vector3.one, Vector3.Project(positionToGo, Forward));
		Clamp();
	}

	protected void Clamp()
	{
		zoomToGo = Mathf.Clamp(zoomToGo, ZoomBounds.x, ZoomBounds.y);
		if (PanBounds != null)
		{
			float distance = Mathf.Abs(zoomToGo);
			float angle = Vector3.Dot(Vector3.one, Vector3.Project(rotationToGo.eulerAngles, Forward));
			Vector2 frustum = new Vector2();
			frustum.y = distance;
			if (!cam.orthographic)
				frustum *= Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
			frustum.x = frustum.y * cam.aspect;
			frustum = MathHelper.Rotate(frustum, angle);
			frustum *= 1 - (0.5f * Mathf.Sin(((angle % 91) * 2) * Mathf.Deg2Rad));
			frustum.x = Mathf.Abs(frustum.x);
			frustum.y = Mathf.Abs(frustum.y);
			Vector3 plane = Vector3.ProjectOnPlane(positionToGo, Forward);
			Vector3 extra = Vector3.Project(positionToGo, Forward);
			plane.x = Mathf.Clamp(plane.x, bounds.min.x + frustum.x, bounds.max.x - frustum.x);
			plane.y = Mathf.Clamp(plane.y, bounds.min.y + frustum.y, bounds.max.y - frustum.y);
			plane.z = Mathf.Clamp(plane.z, bounds.min.z + frustum.y, bounds.max.z - frustum.y);
			positionToGo = plane + extra;
		}
	}

	protected void OnTouch(object sender, EventArgs e)
	{
		if (gesture.ActiveTouches.Count == 1)
		{
			Pan(gesture.NormalizedScreenPosition, gesture.PreviousNormalizedScreenPosition);
		}
		else if (gesture.ActiveTouches.Count == 2)
		{
			TouchPoint touch1 = gesture.ActiveTouches[0];
			TouchPoint touch2 = gesture.ActiveTouches[1];

			PinchZoom(touch1.Position, touch2.Position, touch1.PreviousPosition, touch2.PreviousPosition);
			TwistRotate(touch1.Position, touch2.Position, touch1.PreviousPosition, touch2.PreviousPosition);
		}
		Clamp();
	}

	protected void Pan(Vector2 position, Vector2 previousPosition)
	{
		Vector3 delta = previousPosition - position;
		positionToGo += transform.TransformDirection(delta * Mathf.Abs(zoomToGo) * PanSpeed);
	}

	protected void PinchZoom(Vector2 position1, Vector2 position2, Vector2 previousPosition1, Vector2 previousPosition2)
	{
		Vector2 viewSize = cam.pixelRect.size;
		float previousDeltaMagnitude = (previousPosition1 - previousPosition2).magnitude;
		float deltaMagnitude = (position1 - position2).magnitude;
		float deltaMagnitudeDifference = previousDeltaMagnitude - deltaMagnitude;
		if (!cam.orthographic && Vector3.Dot(Vector3.one, Forward) > 0)
			deltaMagnitudeDifference *= -1;
		float newZoomToGo = zoomToGo + (deltaMagnitudeDifference * ZoomSpeed);
		if (newZoomToGo > ZoomBounds.y || newZoomToGo < ZoomBounds.x)
			return;
		float absNewZoomToGo = Mathf.Abs(newZoomToGo);
		positionToGo += transform.TransformDirection((previousPosition1 + previousPosition2 - viewSize) * absNewZoomToGo / viewSize.y);
		zoomToGo = newZoomToGo;
		positionToGo -= transform.TransformDirection((position1 + position2 - viewSize) * absNewZoomToGo / viewSize.y);
	}

	protected void TwistRotate(Vector2 position1, Vector2 position2, Vector2 previousPosition1, Vector2 previousPosition2)
	{
		float angle = MathHelper.AngleBetween(position1, position2);
		float previousAngle = MathHelper.AngleBetween(previousPosition1, previousPosition2);
		float deltaAngle = Mathf.DeltaAngle(angle, previousAngle);
		rotationToGo = Quaternion.AngleAxis(deltaAngle * RotateSpeed, Forward) * rotationToGo;
	}

	protected void LateUpdate()
	{
		float fraction = Smoothing * Time.deltaTime;
		if (cam.orthographic)
			cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, zoomToGo, fraction);
		else
			positionToGo = Vector3.ProjectOnPlane(positionToGo, Forward) + Forward * zoomToGo;
		transform.position = Vector3.Lerp(transform.position, positionToGo, fraction);
		transform.rotation = Quaternion.Slerp(transform.rotation, rotationToGo, fraction);
	}
}
*/