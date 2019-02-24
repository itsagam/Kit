using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Async;
using Modding;

public class Test : MonoBehaviour
{
	public Window Window;

	protected void Start()
	{
		ModManager.LoadMods();
	}

	public void Button()
	{
		UIManager.ShowWindow(Window).Forget();
		/*
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
		*/
	}
}
