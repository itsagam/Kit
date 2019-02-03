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
using Game;

// GameState:
//---------------------------------------
// JObject and PopulateObject
//	[Prefab] attribute to tell which prefab to use while instantiating, may use replacements like "{Type}" in the path
//		Pros: Works with inhereted types, manually work is not needed, properties show in inspector
//		Cons: Changing Game Data would be harder since properties are duplicated

// JSON
//		Pros: Works with inhereted types, game state modifications are instant & easy since there's only one copy
//		Cons: Have to manually write property accessors, properties don't show in inspector

// GameData:
//---------------------------------------
// Static-fields (to be modifiable in Lua)
//		Pros: No extra work required, properties show in inspector
//		Cons: Can't find a way to work with inhereted types (statically bound), game state still has to be serialized

// Constant values (to be modifiable in Lua)
//		Pros: Easiest way to develop, properties show in inspector, works with inhereted types
//		Cons: Have to load prefabs before values can be modified, game state still has to be serialized

[Hotfix]
public class Test : MonoBehaviour
{	
	public GameObject cube;
	//JObject jState;
	GameState GameState;


#pragma warning disable CS1998
	void Start()
	{
		//ModdingTest();
		//await DataManager.LoadData();

		GameState = ResourceManager.Load<GameState>(DataManager.DataFolder, DataManager.GameStateFile);
		Debugger.Log(GameState);
		//GameState.Enemies.ForEach(e => Debugger.Log(e));

		/*
		string json = await ResourceManager.ReadTextAsync(ResourceFolder.StreamingAssets, DataManager.GameStateFile);
		var jSerializer = JsonSerializer.CreateDefault();
		jState = JObject.Parse(json);
		foreach (var jEnemy in jState["Enemies"].Children())
		{
			var instance = Instantiate(ResourceManager.Load<Enemy>(ResourceFolder.Resources, "Enemies/" + jEnemy["Type"].Value<string>()));
			using (var jReader = jEnemy.CreateReader())
				jSerializer.Populate(jReader, instance);

			var serializer = instance.gameObject.AddComponent<Serializer>();
			serializer.Token = jEnemy;
			serializer.Object = instance;
		}
		*/
	}
#pragma warning restore CS1998

	void OnDestroy()
	{
		//FindObjectsOfType<Serializer>().ForEach(s => s.Serialize());
		//Debugger.Log(jState.ToString());
		//ModManager.UnloadMods();
	}

	public void RunProfile()
	{
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
}
