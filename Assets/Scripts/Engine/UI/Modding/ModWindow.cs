using UnityEngine;
using UnityEngine.UI;
using Modding;
using UniRx.Async;

public class ModWindow : Window
{
	public IconList ModList;
	public Text CountText;
	public Window MessageWindow;

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
		UIManager.ShowWindow(MessageWindow, "Some changes will not be reflected until you restart the application.").Forget();
	}

	public static void SetDirty()
	{
		UIManager.FindWindow<ModWindow>().IsDirty = true;
	}
#endif
}