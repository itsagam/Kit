using Engine;
using Engine.Modding;
using UnityEngine;

namespace Game
{
	public class Test : MonoBehaviour
	{
		private void Awake()
		{
			ModManager.LoadMods();
		}

		public void Button()
		{
			UIManager.Show(Windows.Mods);
		}
	}
}