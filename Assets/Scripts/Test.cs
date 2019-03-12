using Engine;
using Engine.Modding;
using UnityEngine;

namespace Game
{
	public class Test : MonoBehaviour
	{
		private async void Awake()
		{
			await ModManager.LoadModsAsync(true);
			print(ModManager.ActiveMods.Count);
		}

		public void Button()
		{
			UIManager.Show(Windows.Mods);
		}
	}
}