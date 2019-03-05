using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Async;
using Modding;
using XLua;

public class Test : MonoBehaviour
{
	protected void Awake()
	{
		ModManager.LoadMods(true);	
	}

	protected void Start()
	{
		//LuaEnv env = new LuaEnv();
		//env.DoString("print(\"Agam\")");
		//env.Dispose();
	}

	public void Button()
	{
		ModManager.UnloadMods();
	}
}
