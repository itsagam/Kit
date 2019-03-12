using System.Linq;
using Engine;
using Engine.Modding;
using Engine.Modding.Loaders;
using UnityEngine;

namespace Game
{
	public class Test : MonoBehaviour
	{
		private async void Awake()
		{
			await ModManager.LoadModsAsync(true);
			print(ModManager.ActiveMods.Count);
			Debugger.Log(await ModManager.ActiveMods.OfType<AssetBundleMod>().First().LoadAsync<ModMetadata>("Metadata.json"),
			true);
			Debugger.Log(await ModManager
						.ActiveMods
						.OfType<AssetBundleMod>()
						.First()
						.LoadAsync<Texture>("Resources/Textures/test"));
			Debugger.Log(await ModManager
						.ActiveMods
						.OfType<AssetBundleMod>()
						.First()
						.LoadAsync<Texture>("Resources/Textures/test.jpeg"));
		}

		public void Button()
		{
			//UIManager.Show(Windows.Mods);
		}
	}
}