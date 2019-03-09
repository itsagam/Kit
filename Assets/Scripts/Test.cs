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
		Do(null);
	}

	protected void Do(Card card)
	{
		Card c1 = null;
		print(c1 >= card );
	}

	public void Button()
	{

	}
}
