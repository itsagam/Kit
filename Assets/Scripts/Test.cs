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
	public GameObject obj;
	public Text text;

	async void Start()
	{
		await LoadMods();

		Debugger.Log("Full", await ResourceManager.LoadAsync<GameData>(ResourceFolder.StreamingAssets, "Data/Test.json", true), true);

		//Texture tex = await ResourceManager.LoadAsync<Texture>(ResourceFolder.Resources, @"Textures/Test");
		//obj.GetComponent<MeshRenderer>().material.mainTexture = tex;

		//AudioClip clip = await ResourceManager.LoadAsync<AudioClip>(ResourceFolder.Resources, @"Sounds/Test");
		//GetComponent<AudioSource>().clip = clip;

		//Debugger.Log(ResourceManager.GetLoadedModFileInfo(ResourceFolder.Resources, @"Textures/Test")[0].Parser);
	}

	protected async Task LoadMods()
	{
		await ModManager.LoadModsAsync();
		foreach (ModPackage mod in ModManager.ModPackages)
			Debug.Log(mod.Metadata.Name);
		ModManager.ResourceLoaded += ModManager_ResourceLoaded;
		ModManager.ResourceReused += ModManager_ResourceReused;
	}

	protected void ProfileTest()
	{
		Debugger.StartProfile("Normal");
		for (int i = 0; i < 100000; i++)
			Resources.Load<Texture>("Textures/test");
		Debugger.EndAndLogProfile();
	}

	protected void ScriptingTest()
	{
		/*
		foreach(var f in Assembly.GetExecutingAssembly().GetTypes().SelectMany(t => t.GetFields().Where(p => Attribute.IsDefined(p, typeof(VariableAttribute)))))
		{
		if (f.IsStatic)
			Debug.Log(f.GetValue(null));
		else
			Debug.Log(f.GetValue(FindObjectOfType(f.DeclaringType)));
		}
		*/
		/*
		foreach (var obj in Resources.FindObjectsOfTypeAll<Test>())
			obj.Value2 = 20;

		foreach (var obj in Resources.FindObjectsOfTypeAll<Test>())
			Debug.Log(obj.Value2);
		*/
		/*
		Debug.Log(Resources.Load<Test>("Prefab").Value2);
		
		if (count <= 0)
		{
			Instantiate<Test>(Resources.Load<Test>("Prefab"));
			count++;
		}
		*/
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