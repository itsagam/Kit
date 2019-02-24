using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Modding;

// TODO: Reload window when values are changed
// TODO: Add "Apply Changes" button
// TODO: Show "Changes will be reflected in the next boot-up"

// TODO: Make UI texts pleasing
// TODO: Add separator
// TODO: Add Enabled/Disabled status text
// TODO: Add UI animations
// TODO: Add UI sounds

public class ModWindow : Window
{
	public IconList List;
	public Text CountText;

	public override void Reload()
	{
		var mods = ModManager.GetModsByGroup(ModType.Mod);
		List.Items = mods;
		CountText.text = $"{mods.Count} mod(s) found";
	}
}