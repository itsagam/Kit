using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Modding;
using DG.Tweening;

public class ModIcon : Icon
{
	public Text NameText;
	public Text VersionText;
	public Text AuthorText;
	public Text DescriptionText;
	public Toggle EnableToggle;
	public Button MoveUpButton;
	public Button MoveDownButton;

	public Color EnabledColor;
	public Color DisabledColor;
	public float RecolorTime = 0.35f;

	protected ModWindow window;

	protected void Awake()
	{
		window = UIManager.FindWindow<ModWindow>();

		EnableToggle.onValueChanged.AddListener(Toggle);
		MoveUpButton.onClick.AddListener(MoveUp);
		MoveDownButton.onClick.AddListener(MoveDown);
	}

	public override void Reload()
	{
		EnableToggle.isOn = ModManager.IsModEnabled(Mod);

		var list = ModManager.GetModsByGroup(ModType.Mod);
		if (list[0] == Mod)
			MoveUpButton.SetInteractableImmediate(false);

		if (list[list.Count - 1] == Mod)
			MoveDownButton.SetInteractableImmediate(false);

		var metadata = Mod.Metadata;
		NameText.text = metadata.Name;
		VersionText.text = metadata.Version;
		AuthorText.text = metadata.Author;
		DescriptionText.text = metadata.Description;

		NameText.color = EnableToggle.isOn ? EnabledColor : DisabledColor;
	}

	protected void MoveUp()
	{
		ModManager.MoveModUp(Mod);
		window.Reload();
	}

	protected void MoveDown()
	{
		ModManager.MoveModDown(Mod);
		window.Reload();
	}

	protected void Toggle(bool value)
	{
		ModManager.ToggleMod(Mod, value);
		NameText.DOColor(value ? EnabledColor : DisabledColor, RecolorTime);
	}

	public Mod Mod
	{
		get
		{
			return (Mod) Data;
		}
	}
}