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

public class GameData
{
	public string Mod0;
	public string Mod1;
	public string Mod2;
	public string Mod3;
	public double Value;
}

public class Test2: Test
{
	public static string String = "Default";
}

public class Test: MonoBehaviour
{
	public static int Integer = 1;
	public static float Float = 1.5f;
	public static void Call()
	{
		print("Called");
	}

	public GameObject obj;

	async void Start()
	{
		//await LoadMods();
		//await ModdingTest();

		//await ConsoleTest();
	}

	protected async Task LoadMods()
	{
		await ModManager.LoadModsAsync();
		foreach (ModPackage mod in ModManager.ModPackages)
			Debug.Log(mod.Metadata.Name);
		ModManager.ResourceLoaded += ModManager_ResourceLoaded;
		ModManager.ResourceReused += ModManager_ResourceReused;
	}

	protected async Task ModdingTest()
	{
		Debugger.Log(await ResourceManager.LoadAsync<GameData>(ResourceFolder.StreamingAssets, "Data/Test.json", true), true);

		Texture tex = await ResourceManager.LoadAsync<Texture>(ResourceFolder.Resources, @"Textures/Test");
		obj.GetComponent<MeshRenderer>().material.mainTexture = tex;

		AudioClip clip = await ResourceManager.LoadAsync<AudioClip>(ResourceFolder.Resources, @"Sounds/Test");
		GetComponent<AudioSource>().clip = clip;
		//GetComponent<AudioSource>().Play();
	}

	protected void ProfileTest()
	{
		Debugger.StartProfile("Normal");
		for (int i = 0; i < 100000; i++)
			Resources.Load<Texture>("Textures/test");
		Debugger.EndAndLogProfile();
	}

	async Task ConsoleTest()
	{
		for (int i = 0; i < 30; i++)
		{
			Debug.Log("Log " + i);
			await Observable.Timer(TimeSpan.FromSeconds(0.1f));
		}
	}

	private void ModManager_ResourceReused(string path, ResourceInfo info)
	{
		Debug.Log($"File \"{path}\" resused from \"{info.Package.Path}\"");
	}

	private void ModManager_ResourceLoaded(string path, ResourceInfo info)
	{
		Debug.Log($"File \"{path}\" loaded from \"{info.Package.Path}\"");
	}

	void OnDestroy()
	{
		//ModManager.UnloadMods();
	}
}