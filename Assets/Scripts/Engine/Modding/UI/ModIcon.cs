using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Modding;

public class ModIcon : Icon
{
	public Text NameText;
	public Text VersionText;
	public Text AuthorText;
	public Text DescriptionText;
	public Toggle EnableToggle;
	public Button MoveUpButton;
	public Button MoveDownButton;

	protected void Start()
	{
		MoveUpButton.onClick.AddListener(() => ModManager.MoveModUp(Mod));
		MoveDownButton.onClick.AddListener(() => ModManager.MoveModDown(Mod));
		EnableToggle.onValueChanged.AddListener((value) => ModManager.ToggleMod(Mod, value));
	}

	public override void Reload()
	{
		EnableToggle.isOn = ModManager.IsModEnabled(Mod);

		var metadata = Mod.Metadata;
		NameText.text = metadata.Name;
		VersionText.text = metadata.Version;
		AuthorText.text = metadata.Author;
		DescriptionText.text = metadata.Description;
	}

	public Mod Mod
	{
		get
		{
			return (Mod) Data;
		}
	}
}