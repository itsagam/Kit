using Engine;
using Engine.UI;
using Engine.UI.Widgets;
using UnityEngine.UI;

#if MODDING
using Engine.Modding;
#endif

namespace Game.UI.Mods
{
	public class ModWindow: Window
	{
		public IconList ModList;
		public Text CountText;
		public WindowReference MessageWindow;

#if MODDING
		public bool IsDirty { get; set; }
		public bool IsAnimating { get; set; }

		public override void Refresh()
		{
			var mods = ModManager.GetModsByGroup(ModType.Mod);
			CountText.text = $"{mods.Count} mod(s) found";
			ModList.Items = mods;
			IsDirty = false;
		}

		protected override void OnHidden()
		{
			SaveChanges();
		}

		protected void SaveChanges()
		{
			if (!IsDirty)
				return;

			PreferenceManager.Save();
			UIManager.Show(MessageWindow, "Some changes will not be reflected until you restart the application.");
		}
#endif
	}
}