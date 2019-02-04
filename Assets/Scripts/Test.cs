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

[Hotfix]
public class Test : MonoBehaviour
{	
	public GameObject cube;

#pragma warning disable CS1998
	void Start()
	{
		//ModdingTest();
		//await DataManager.LoadData();
	}
#pragma warning restore CS1998

	void OnDestroy()
	{
		//ModManager.UnloadMods();
	}

	public static object Load<T>(string file) where T: UnityEngine.Object
	{
		return ResourceManager.Load<Texture>(ResourceFolder.Resources, file, true, false);
	}

	public void RunProfile()
	{
		var enemy = Instantiate(ResourceManager.Load<Enemy>(ResourceFolder.Resources, "Enemies/SwordEnemy"));
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
}
