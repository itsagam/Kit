using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
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

	[SerializeField]
	[HideInInspector]
	protected float rating = 0;

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

		button.OnPointerUpAsObservable().Subscribe(OnClick);

		buttons[index] = button;
	}

	protected void OnClick(PointerEventData data)
	{
		if (IsReadonly)
			return;

		RectTransform rect = (RectTransform) data.selectedObject.transform;
		int index = rect.GetSiblingIndex();
		if (AllowHalf)
		{
			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, data.pressPosition, data.pressEventCamera, out Vector2 point))
				return;

			float decimalPart = point.x < 0 ? 0.5f : 1.0f;
			SetRating(index + decimalPart);
		}
		else
		{
			SetRating(index + 1);
		}
	}

	protected void SetRating(float newRating)
	{
		if (newRating < 0 || newRating > MaxRating)
			return;

		int intPart = (int) newRating;
		float decimalPart = newRating % 1;
		bool half = AllowHalf ? decimalPart >= 0.5f : false;

		if (half)
			rating = intPart + 0.5f;
		else
			rating = intPart;

		if (!Application.isPlaying)
			return;
			
		for (int i = 0; i < intPart; i++)
			buttons[i].image.sprite = OneSprite;

		if (intPart < MaxRating)
		{
			buttons[intPart].image.sprite = half ? HalfSprite : ZeroSprite;
			for (int i = intPart + 1; i < MaxRating; i++)
				buttons[i].image.sprite = ZeroSprite;
		}
	}

	[ShowInInspector]
	[PropertyOrder(-1)]
	[PropertyRange(0, "GetMaxRatingAsFloat")]
	public float Rating
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

#if UNITY_EDITOR
	protected float GetMaxRatingAsFloat()
	{
		return MaxRating;
	}
#endif
}