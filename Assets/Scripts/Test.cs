using System;
using System.Collections;
using System.Collections.Generic;
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
using Newtonsoft.Json.Linq;

// JObject
//	[Prefab] attribute to tell which prefab to use while instantiating
//	[GameData] and [GameState] attribute on fields to tell which fields to use in which file while serializing
//	(or not? GameData is not serialized...)
//		Pros: Works with inhereted types, manually work is not needed, properties show in inspector
//		Cons: Have to go through files once, changing Game Data would be harder since properties are duplicated

// JSON:
//		Pros: Works with inhereted types
//		Cons: Have to manually write property accessors, properties don't show in inspector

// Static-fields to be modifiable in Lua
//		Pros: No extra work required, properties show in inspector
//		Cons: Can't find a way to work with inhereted types (statically bound)

public static class TestExtensions
{
	public static float Extension(this Test t, int y)
	{
		return 0;
	}
}

[Hotfix]
public class Test : MonoBehaviour
{	
	public GameObject cube;

#pragma warning disable CS1998
	async UniTask Start()
	{
		//ModdingTest();
		//await DataManager.LoadData();

		string stateJson = await ResourceManager.ReadTextAsync(ResourceFolder.StreamingAssets, DataManager.GameStateFile);
		JObject stateObject = JObject.Parse(stateJson);
		var serializer = JsonSerializer.CreateDefault();
		foreach (var child in stateObject["Enemies"].Children())
		{
			var instance = Instantiate(ResourceManager.Load<Enemy>(ResourceFolder.Resources, "Enemy"));
			using (var sr = child.CreateReader())
				serializer.Populate(sr, instance);
		}
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
