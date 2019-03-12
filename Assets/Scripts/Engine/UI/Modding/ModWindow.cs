using UnityEngine;
using UnityEngine.UI;
#if MODDING
using Engine.Modding;
#endif

namespace Engine.UI.Modding
{
	public class ModWindow : Window
	{
		public IconList ModList;
		public Text CountText;

#if MODDING
		public bool IsDirty { get; set; }

		protected override void Awake()
		{
			base.Awake();
			OnWindowHidden.AddListener(SaveChanges);
		}

		public override void Refresh()
		{
			var mods = ModManager.GetModsByGroup(ModType.Mod);
			CountText.text = $"{mods.Count} mod(s) found";
			ModList.Items = mods;
			IsDirty = false;
		}

		protected void SaveChanges()
		{
			if (!IsDirty)
				return;

			PlayerPrefs.Save();
			UIManager.Show(Game.Windows.Message, "Some changes will not be reflected until you restart the application.");
		}

		public static void SetDirty()
		{
			UIManager.Find<ModWindow>().IsDirty = true;
		}
#endif
	}
}