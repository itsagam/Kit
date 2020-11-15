using Kit;
using Kit.UI;

namespace Game.UI.Settings
{
	public class SettingsWindow: Window
	{
		protected override void OnHidden()
		{
			SettingsManager.Save();
		}
	}
}