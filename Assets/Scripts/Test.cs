using System.Collections.Generic;
using Cards;
using UnityEngine;
using Modding;

public class Test : MonoBehaviour
{
	private GameObject go;

	protected void Awake()
	{
		//CS.ResourceManager.Load(typeof(UE.Texture), CS.ResourceFolder.Resources, "Textures/test")
		//ModManager.LoadMods(false);
		go = new GameObject("agam");
	}

	public void Button()
	{
		if (go != null)
			go.transform.SetParent(transform);
	}
}
