using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Async;

public class Test : MonoBehaviour
{
	public Parent Parent;
	public Child Child;

	protected Transform trans1;
	public Transform trans2 { get; protected set; }

	protected void Awake()
	{
		trans1 = transform;
		trans2 = transform;

	}

	public void Button()
	{
		Debugger.StartProfile("transform");
		for (int i = 0; i < 1000000; i++)
		{
			var v = transform;
		}

		Debugger.EndProfile();

		
		Debugger.StartProfile("field");
		for (int i = 0; i < 1000000; i++)
		{
			var v = trans1;
		}
		Debugger.EndProfile();

		Debugger.StartProfile("property");
		for (int i = 0; i < 1000000; i++)
		{
			var v = trans2;
		}

		Debugger.EndProfile();
	}
}
