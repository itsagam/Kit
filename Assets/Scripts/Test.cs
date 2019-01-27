﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using Modding;
using UniRx;
using UniRx.Async;
using XLua;

public class GameData
{
	public string Mod0;
	public string Mod1;
	public string Mod2;
	public string Mod3;
	public double Value;
}

public class Test: MonoBehaviour
{
	public static string String = "Default";
	public static int Integer = 1;
	public static float Float = 1.5f;
	public static void Call()
	{
		print("Called");
	}
	public static void Call(int value)
	{
		print("Called I " + value);
	}
	public static void Call(string value)
	{
		print("Called S " + value);
	}

	public static int PropertyInt
	{
		get
		{
			return 1;
		}
	}

	public static AudioClip global;

	public GameObject cube;

#pragma warning disable CS1998
	 void  Awake()
	{
		ModdingTest();
		//InjectTest()

		//global = new WeakReference(new Modding.Loaders.DirectModLoader());
		//if (global == null)
		//{
		//Texture tex = LoadTexture(ResourceManager.ReadBytes(ResourceFolder.Resources, "Textures/test.jpeg"));
		//var clip = WavUtility.ToAudioClip(ResourceManager.ReadBytes(ResourceFolder.Resources, "Sounds/test.wav"), 0);
		//var text = LoadTextAsset("agam");
		//global = clip;//new WeakReference(clip, false);
		//}
		//print(global); //.Target);
		//SceneManager.ReloadScene();
		
		/*
		Debugger.StartProfile("Resources.Load");
		for (int i = 0; i <= 1000000; i++)
			Resources.Load<Texture>("Textures/test");
		Debugger.EndProfile();

		Debugger.StartProfile("ResourceManager.Load");
		for (int i = 0; i <= 1000000; i++)
			ResourceManager.Load<Texture>(ResourceFolder.Resources, "Textures/test.jpeg", true);
		Debugger.EndProfile();
		*/
	}
#pragma warning restore CS1998

	public TextAsset LoadTextAsset(string data)
	{
		TextAsset asset = new TextAsset(data);
		return asset;
	}

	public Texture LoadTexture(object data)
	{
		Texture2D texture = null;
		texture = new Texture2D(0, 0);
		texture.LoadImage((byte[]) data);
		return texture;
	}

	private void ResourceReused(ResourceFolder folder, string path, object obj)
	{
		Debug.Log($"File \"{path}\" reused from folder \"{folder.ToString()}\"");
	}

	private void ResourceLoaded(ResourceFolder folder, string path, object obj)
	{
		Debug.Log($"File \"{path}\" loaded from folder \"{folder.ToString()}\"");
	}

	protected void ModdingTest()
	{
		ModManager.LoadMods();

		ResourceManager.ResourceLoaded += ResourceLoaded;
		ResourceManager.ResourceReused += ResourceReused;

		ModManager.ResourceLoaded += ModManager_ResourceLoaded;
		ModManager.ResourceReused += ModManager_ResourceReused;

		//Debugger.Log(ResourceManager.Load<GameData>(ResourceFolder.StreamingAssets, @"Data/Test.json", false), true);

		//Texture tex = ResourceManager.Load<Texture>(ResourceFolder.Resources, @"Textures/Test", false);
		//cube.GetComponent<MeshRenderer>().material.mainTexture = tex;

		//AudioClip clip = ResourceManager.Load<AudioClip>(ResourceFolder.Resources, @"Textures/Test", true);
		//GetComponent<AudioSource>().clip = clip;
		//GetComponent<AudioSource>().Play();
	}

	private void ModManager_ResourceReused(string path, ResourceInfo info)
	{
		Debug.Log($"File \"{path}\" resused from \"{info.Mod.Path}\"");
	}

	private void ModManager_ResourceLoaded(string path, ResourceInfo info)
	{
		Debug.Log($"File \"{path}\" loaded from \"{info.Mod.Path}\"");
	}

	void OnDestroy()
	{
		//ModManager.UnloadMods();
	}

	void InjectTest()
	{
		LuaEnv luaenv = new LuaEnv();
		luaenv.DoString(@"
				local util = require 'xlua.util'
				util.hotfix_ex(CS.Test, 'Hello', function(self)
					self:Hello()
                    print('Hello from Lua')
                end)
            ");
		Hello();
	}
	
	public void Hello()
	{
		Debug.Log("Hello from C#");
	}
}