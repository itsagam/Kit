using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class RatingPicker : MonoBehaviour
{
	[Range(1, 10)]
	public int MaxRating = 5;	
	public bool AllowHalf = true;
	public bool IsReadonly = false;

	[FoldoutGroup("Sprites")]
	public Sprite ZeroSprite;
	[FoldoutGroup("Sprites")]
	[ShowIf("AllowHalf")]
	public Sprite HalfSprite;
	[FoldoutGroup("Sprites")]
	public Sprite OneSprite;

	[FoldoutGroup("Appearance")]
	public Color HighlightedColor = new Color(0.9f, 0.9f, 0.9f, 1);
	[FoldoutGroup("Appearance")]
	public Color PressedColor = new Color(0.75f, 0.75f, 0.75f, 1);
	[FoldoutGroup("Appearance")]
	public float Spacing = 10;

	protected HorizontalLayoutGroup layout;
	protected Button[] buttons;

	protected void Awake()
	{
		SetupLayout();
		SpawnButtons();
		SetRating(rating);
	}

	protected void SetupLayout()
	{
		layout = gameObject.AddComponent<HorizontalLayoutGroup>();
		layout.spacing = Spacing;
	}

	protected void SpawnButtons()
	{
		buttons = new Button[MaxRating];
		for (int i = 0; i < MaxRating; i++)
			SpawnButton(i);
	}

	protected void SpawnButton(int index)
	{
		GameObject spriteGO = new GameObject("Button " + (index + 1), typeof(Image));
		Button button = spriteGO.AddComponent<Button>();
		button.image.sprite = ZeroSprite;
		button.transform.SetParent(transform, false);
		button.interactable = !IsReadonly;
		
		var colors = button.colors;
		colors.disabledColor = colors.normalColor;
		colors.highlightedColor = HighlightedColor;
		colors.pressedColor = PressedColor;
		button.colors = colors;
		button.onClick.AddListener(() => SetRating(index + 1));

		buttons[index] = button;
	}

	protected void SetRating(int newRating)
	{
		for (int i = 0; i < newRating; i++)
			buttons[i].image.sprite = OneSprite;

		for (int i = newRating; i < MaxRating; i++)
			buttons[i].image.sprite = ZeroSprite;

		rating = newRating;
	}
	
	[SerializeField]
	[HideInPlayMode]
	[PropertyRange(0, "MaxRating")]
	protected int rating;

	[ShowInInspector]
	[HideInEditorMode]
	[PropertyRange(0, "MaxRating")]
	public int Rating
	{
		get
		{
			return rating;
		}
		set
		{
			SetRating(value);
		}
	}
}