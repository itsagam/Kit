﻿using System;
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
		ModManager.LoadMods();
		//CS.ResourceManager.Load(typeof(UE.Texture), CS.ResourceFolder.Resources, "Textures/test")
	}

	protected void Start()
	{
	}

	public void Button()
	{
	}
}
