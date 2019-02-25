using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Modding;

// TODO: Add "Apply Changes" button
// TODO: Show "Changes will be reflected in the next boot-up"

// TODO: Make checkbox pleasing
// TODO: Make consistent color scheme
// TODO: Organize, trim and optimize Clean UI asset
// TODO: Make prefabs for UI elements

public class ModWindow : Window
{
	public IconList ModList;
	public Text CountText;

	public override void Reload()
	{
		var mods = ModManager.GetModsByGroup(ModType.Mod);
		ModList.Items = mods;
		CountText.text = $"{mods.Count} mod(s) found";
	}
}