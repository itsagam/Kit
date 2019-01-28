using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

	public GameObject cube;

#pragma warning disable CS1998
	async Task Start()
	{
		//ModdingTest();
		//InjectTest()

	}

#pragma warning restore CS1998

	public void RunProfile()
	{
		Debugger.StartProfile("Resources.Load");
		for (int i = 0; i <= 100000; i++)
			Resources.Load<Texture>("Textures/test");
		Debugger.EndProfile();

		Debugger.StartProfile("ResourceManager.Load");
		for (int i = 0; i <= 100000; i++)
			ResourceManager.LoadUnmodded<Texture>(ResourceFolder.Resources, "Textures/test");
		Debugger.EndProfile();
	}

	protected static void ModdingTest()
	{
		ModManager.LoadMods();

		//ResourceManager.ResourceLoaded += ResourceLoaded;
		//ResourceManager.ResourceReused += ResourceReused;
		//ResourceManager.ResourceUnloaded += ResourceUnloaded;

		//ModManager.ResourceLoaded += ModManager_ResourceLoaded;
		//ModManager.ResourceReused += ModManager_ResourceReused;
		//ModManager.ResourceUnloaded += ModManager_ResourceUnloaded;
		//Debugger.Log(ResourceManager.Load<GameData>(ResourceFolder.StreamingAssets, @"Data/Test.json", false), true);

		//Texture tex = ResourceManager.Load<Texture>(ResourceFolder.Resources, @"Textures/Test", true);
		//cube.GetComponent<MeshRenderer>().material.mainTexture = tex;

		//AudioClip clip = ResourceManager.Load<AudioClip>(ResourceFolder.Resources, @"Textures/Test", true);
		//GetComponent<AudioSource>().clip = clip;
		//GetComponent<AudioSource>().Play();
	}

	private static void ResourceReused(string path, object obj)
	{
		Debug.Log($"File \"{path}\" reused.");
	}

	private static void ResourceLoaded(string path, object obj)
	{
		Debug.Log($"File \"{path}\" loaded.");
	}

	private static void ResourceUnloaded(string path)
	{
		Debug.Log($"File \"{path}\" unloaded.");
	}

	private static void ModManager_ResourceUnloaded(string path, Mod mod)
	{
		Debug.Log($"File \"{path}\" unloaded from \"{mod.Path}\"");
	}

	private static void ModManager_ResourceReused(string path, ResourceInfo info)
	{
		Debug.Log($"File \"{path}\" resused from \"{info.Mod.Path}\"");
	}

	private static void ModManager_ResourceLoaded(string path, ResourceInfo info)
	{
		Debug.Log($"File \"{path}\" loaded from \"{info.Mod.Path}\"");
	}

	void OnDestroy()
	{
		ModManager.UnloadMods();
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