using Engine;
using Engine.UI;

namespace Game.UI.Settings
{
	public class SettingsWindow : Window
	{
		protected override void OnHidden()
		{
			PreferenceManager.Save();
		}
	}
}