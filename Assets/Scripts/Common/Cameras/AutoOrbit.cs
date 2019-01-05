using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoOrbit: MonoBehaviour
{
	public Transform Target;
	public Vector3 Axis = Vector3.up;
	public float Speed = 10.0f;

	protected void LateUpdate()
	{
		if (Target == null)
			return;
		
		transform.RotateAround(Target.position, Axis, Time.deltaTime * Speed);
	}
}