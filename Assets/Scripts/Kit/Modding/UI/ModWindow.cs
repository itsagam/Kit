using Kit.UI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

#if MODDING

#endif

namespace Kit.Modding.UI
{
	/// <summary>A pre-written <see cref="Window" /> for managing mods in-game.</summary>
	public class ModWindow: Window
	{
		/// <summary>List to use for populating mods. Provide a panel with a <see cref="ItemList" /> referencing a <see cref="ModItem" /> prefab.</summary>
		[Tooltip("List to use for populating mods. Provide a panel with a ItemList referencing a ModItem prefab.")]
		[SceneObjectsOnly]
		public ItemList ModList;

		/// <summary>Label to use for showing the number of mods loaded.</summary>
		[Tooltip("Label to use for showing the number of mods loaded.")]
		[SceneObjectsOnly]
		public Text CountText;

		/// <summary><see cref="MessageWindow" /> to use for showing messages.</summary>
		[Tooltip("MessageWindow to use for showing messages.")]
		public WindowReference MessageWindow;

#if MODDING
		/// <summary>Returns whether any changes were made.</summary>
		public bool IsDirty { get; set; }

		/// <summary>Returns whether the UI is busy.</summary>
		public bool IsAnimating { get; set; }

		/// <summary>(Re)populate the mods list.</summary>
		public override void Refresh()
		{
			var mods = ModManager.GetModsByGroup(ModType.Mod);
			CountText.text = $"{mods.Count} mod(s) found";
			ModList.Data = mods;
			IsDirty = false;
		}

		protected override void OnHidden()
		{
			SaveChanges();
		}

		/// <summary>Save any changes made to the mods.</summary>
		public void SaveChanges()
		{
			if (!IsDirty)
				return;

			SettingsManager.Save();
			UIManager.Show(MessageWindow, "Some changes will not be reflected until you restart the application.");
		}
#endif
	}
}