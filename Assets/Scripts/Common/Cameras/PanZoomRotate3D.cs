using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;

[RequireComponent(typeof(ScreenTransformGesture))]
[RequireComponent(typeof(Camera))]
public class PanZoomRotate3D: MonoBehaviour
{
	public float PanSpeed = 0.005f;
	public Transform PanBounds;
	public float ZoomSpeed = 10.0f;
	public Vector2 ZoomBounds = new Vector2(2, 15);
	public float RotateSpeed = 0.5f;
	public float Smoothing = 10.0f;

	protected ScreenTransformGesture gesture;
	protected Camera cam;
	protected Bounds bounds;
	protected Vector3 positionToGo;
	protected Quaternion rotationToGo;

	protected void Awake()
	{
		gesture = GetComponent<ScreenTransformGesture>();
		cam = GetComponent<Camera>();
		if (PanBounds != null)
			bounds = PanBounds.GetBounds();
	}

	protected void OnEnable()
	{
		gesture.Transformed += OnTransform;
		Refresh();
	}

	protected void OnDisable()
	{
		gesture.Transformed -= OnTransform;
	}

	public void Refresh()
	{
		positionToGo = transform.position;
		rotationToGo = transform.rotation;
		Clamp();
	}

	protected void Clamp()
	{
		positionToGo.y = Mathf.Clamp(positionToGo.y, ZoomBounds.x, ZoomBounds.y);
		if (PanBounds != null)
		{
			float distance = positionToGo.y;
			float angle = rotationToGo.eulerAngles.y;
			Vector2 frustum;
			frustum.y = distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
			frustum.x = frustum.y * cam.aspect;
			frustum = MathHelper.Rotate(frustum, angle);
			frustum *= 1 - (0.5f * Mathf.Sin(((angle % 91) * 2) * Mathf.Deg2Rad));
			frustum.x = Mathf.Abs(frustum.x);
			frustum.y = Mathf.Abs(frustum.y);
			positionToGo.x = Mathf.Clamp(positionToGo.x, bounds.min.x + frustum.x, bounds.max.x - frustum.x);
			positionToGo.z = Mathf.Clamp(positionToGo.z, bounds.min.z + frustum.y, bounds.max.z - frustum.y);
		}
	}

	protected void OnTransform(object sender, EventArgs e)
	{
		if (!Mathf.Approximately(gesture.DeltaRotation, 0) || !Mathf.Approximately(gesture.DeltaScale, 1))
		{
			positionToGo.y += (1f - gesture.DeltaScale) * ZoomSpeed;
			rotationToGo = Quaternion.AngleAxis(MathHelper.ClampDeltaAngle(gesture.DeltaRotation) * RotateSpeed, Vector3.up) * rotationToGo;
		}
		else
			positionToGo += transform.TransformDirection(-gesture.DeltaPosition * positionToGo.y * PanSpeed).SetY(0);
		Clamp();
	}

	protected void LateUpdate()
	{
		float fraction = Smoothing * Time.deltaTime;
		transform.position = Vector3.Lerp(transform.position, positionToGo, fraction);
		transform.rotation = Quaternion.Slerp(transform.rotation, rotationToGo, fraction);
	}
}