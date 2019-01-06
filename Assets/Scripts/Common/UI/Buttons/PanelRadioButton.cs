using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class PanelRadioButton : RadioButton
{
	public RectTransform Panel;

	protected override void Awake()
	{
		base.Awake();
		Panel?.gameObject.SetActive(false);
	}

	public override void Select()
	{
		base.Select();
		PanelRadioButton[] siblings = transform.parent.GetComponentsInChildren<PanelRadioButton>();
		foreach (PanelRadioButton sibling in siblings)
			sibling.Panel.gameObject.SetActive(false);
		Panel.gameObject.SetActive(true);
	}
}