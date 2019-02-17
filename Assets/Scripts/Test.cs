using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Async;

public class Test : MonoBehaviour
{
	public Parent Cube;

	protected void Awake()
	{

	}

	public void Button()
	{
		Debugger.StartProfile(".transform");
		for (int i = 0; i < 10000; i++)
			Cube.transform.position = Vector3.one;
		Debugger.EndProfile();

		Transform t = Cube.transform;

		Debugger.StartProfile("Cached");
		for (int i = 0; i < 10000; i++)
			t.position = Vector3.one;
		Debugger.EndProfile();
	}
}
