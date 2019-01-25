using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using Modding;
using UnityEngine.UI;
using UniRx;
using UniRx.Async;
using XLua;
using System.Text;

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

	public GameObject obj;

#pragma warning disable CS1998
	 void  Start()
	{
		ModdingTest();

		ResourceManager.ResourceLoaded += ResourceLoaded;
		ResourceManager.ResourceReused += ResourceReused;

		ResourceManager.Load<Texture>(ResourceFolder.Resources, "Textures/test", false);
		ResourceManager.Load<AudioClip>(ResourceFolder.Resources, "Sounds/test", false);
		ResourceManager.Load<Texture>(ResourceFolder.StreamingAssets, "Textures/test", false);

		//InjectTest();
		//await ConsoleTest();

		
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
		//ModManager.ResourceLoaded += ModManager_ResourceLoaded;
		//ModManager.ResourceReused += ModManager_ResourceReused;

		//Debugger.Log(ResourceManager.Load<GameData>(ResourceFolder.StreamingAssets, @"Data/Test", true), true);

		//Texture tex = ResourceManager.Load<Texture>(ResourceFolder.StreamingAssets, @"Textures/test");
		//obj.GetComponent<MeshRenderer>().material.mainTexture = tex;

		//AudioClip clip = ResourceManager.Load<AudioClip>(ResourceFolder.Resources, @"Sounds/Test");
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
		ModManager.UnloadMods();
	}

	async UniTask ConsoleTest()
	{
		for (int i = 0; i < 30; i++)
			await Observable.Timer(TimeSpan.FromSeconds(0.1f));
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