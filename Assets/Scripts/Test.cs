using Cards;
using UnityEngine;
using Modding;

public class Test : MonoBehaviour
{
	protected void Awake()
	{
		//CS.ResourceManager.Load(typeof(UE.Texture), CS.ResourceFolder.Resources, "Textures/test")
		ModManager.LoadMods(true);
	}

	public void Button()
	{
	}
}
