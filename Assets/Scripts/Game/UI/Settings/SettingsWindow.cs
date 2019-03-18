using Engine.UI;
using UnityEngine;

namespace Game.UI.Settings
{
	public class SettingsWindow : Window
	{
		protected override void OnHidden()
		{
			PlayerPrefs.Save();
		}
	}
}