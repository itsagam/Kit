using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Async;

public class Test : MonoBehaviour
{
	protected void Start()
	{
		
	}

	public void Button()
	{
		Debugger.StartProfile("transform");
		for (int i = 0; i < 10000; i++)
		{
		}

		Debugger.EndProfile();

		
		Debugger.StartProfile("field");
		for (int i = 0; i < 10000; i++)
		{
		}
		Debugger.EndProfile();
	}
}
