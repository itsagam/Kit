using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Modding;
using Modding.Parsers;
using UniRx;
using UniRx.Async;
using XLua;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

[Hotfix]
public class Test : SerializedMonoBehaviour, IUpgradeable
{
	public GameObject cube;
	public StatBaseProperty Float;
	public StatBaseProperty Hello;
	public Stats Stats;
	[HideReferenceObjectPicker]
	[ListDrawerSettings(CustomAddFunction = "AddUpgrade")]
	public ReactiveCollection<Upgrade> Upgrades;

	protected Upgrade AddUpgrade()
	{
		return new Upgrade();
	}

#pragma warning disable CS1998

	async UniTask Start()
	{
		//ModdingTest();
		/*
		Upgrade u1 = new Upgrade("u1")
		{
			new Effect("Health", "+100"),
			new Effect("Damage", "+50%")
		};
		Upgrades.Add(u1.ID, u1);

		print(Health.CurrentValue);

		Upgrade u2 = new Upgrade("u2")
		{
			new Effect("Health", "+100")
		};
		Upgrades.Add(u2.ID, u2);

		print(Health.CurrentValue);
		*/
	}
#pragma warning restore CS1998

	void OnDestroy()
	{
		//ModManager.UnloadMods();
	}

	public ReactiveCollection<Upgrade> GetUpgrades()
	{
		return Upgrades;
	}

	public void Button()
	{

	}

	protected static void ProfileTest()
	{
		Debugger.StartProfile("ResourceManager.Load");
		for (int i = 0; i < 100000; i++)
			ResourceManager.Load<Texture>(ResourceFolder.Resources, "Textures/test");
		Debugger.EndProfile();

		Debugger.StartProfile("Resources.Load");
		for (int i = 0; i < 100000; i++)
			Resources.Load<Texture>("Textures/test");
		Debugger.EndProfile();
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
