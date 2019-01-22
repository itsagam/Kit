using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class RadioButton : MonoBehaviour
{
	public Button Button { get; protected set; }

	protected Color onColor, offColor;

	protected virtual void Awake()
	{
		Button = GetComponent<Button>();
		Button.onClick.AddListener(Select);
		onColor = Button.colors.highlightedColor;
		offColor = Button.colors.normalColor;
	}

	public virtual void Select()
	{
		ColorBlock block;

		RadioButton[] siblings = transform.parent.GetComponentsInChildren<RadioButton>();
		foreach (RadioButton sibling in siblings)
		{
			Button button = sibling.Button;
			block = button.colors;
			block.normalColor = block.highlightedColor = offColor;
			button.colors = block;
		}
		block = Button.colors;
		block.normalColor = block.highlightedColor = onColor;
		Button.colors = block;
	}
}