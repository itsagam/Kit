using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using Modding;
using Modding.Parsers;
using UniRx;
using UniRx.Async;
using XLua;
using Newtonsoft.Json;

// JSON:
//		Pros: Works with inhereted types
//		Cons: Have to manually write property accessors, properties don't show in inspector

// Static-fields to be modifiable in Lua
//		Pros: No extra work required, properties show in inspector
//		Cons: Can't find a way to work with inhereted types (statically bound)

[Hotfix]
public class Test : MonoBehaviour
{	
	public GameObject cube;

#pragma warning disable CS1998
	async Task Start()
	{
		//ModdingTest();
		await DataManager.LoadData();
		Debugger.Log(DataManager.GameData);

	}
#pragma warning restore CS1998

	public void RunProfile()
	{
		//ModManager.UnloadMods();
		
		/*
		Debugger.StartProfile("Resources.Load");
		for (int i = 0; i <= 100000; i++)
			Resources.Load<Texture>("Textures/test");
		Debugger.EndProfile();

		Debugger.StartProfile("ResourceManager.Load");
		for (int i = 0; i <= 100000; i++)
			ResourceManager.Load<Texture>(ResourceFolder.Resources, "Textures/test");
		Debugger.EndProfile();
		*/
	}

	protected static void ModdingTest()
	{
		ModManager.LoadMods();
		ModManager.ExecuteScripts();
		foreach (Mod mod in ModManager.Mods)
			print(mod.Group.Name + ": " + mod.Metadata.Name);

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

	private static void ResourceReused(ResourceFolder folder, string file, object obj)
	{
		Debug.Log($"File \"{file}\" reused.");
	}

	private static void ResourceLoaded(ResourceFolder folder, string file, object obj)
	{
		Debug.Log($"File \"{file}\" loaded.");
	}

	private static void ResourceUnloaded(ResourceFolder folder, string file)
	{
		Debug.Log($"File \"{file}\" unloaded.");
	}

	private static void ModManager_ResourceUnloaded(ResourceFolder folder, string file, Mod mod)
	{
		Debug.Log($"File \"{file}\" unloaded from \"{mod.Path}\"");
	}

	private static void ModManager_ResourceReused(ResourceFolder folder, string file, ResourceInfo info)
	{
		Debug.Log($"File \"{file}\" resused from \"{info.Mod.Path}\"");
	}

	private static void ModManager_ResourceLoaded(ResourceFolder folder, string file, ResourceInfo info)
	{
		Debug.Log($"File \"{file}\" loaded from \"{info.Mod.Path}\"");
	}

	void OnDestroy()
	{
		ModManager.UnloadMods();
	}
}
