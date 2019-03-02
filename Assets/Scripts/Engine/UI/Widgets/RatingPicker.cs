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

[RequireComponent(typeof(HorizontalLayoutGroup))]
public class RatingPicker : MonoBehaviour
{
	protected Button[] buttons;

	[SerializeField]
	[HideInInspector]
	protected int maxRating = 5;

	[SerializeField]
	[HideInInspector]
	protected float rating = 0;

	[SerializeField]
	[HideInInspector]
	protected bool allowHalf = true;

	[SerializeField]
	[HideInInspector]
	protected bool isReadonly = false;

	[SerializeField]
	[HideInInspector]
	protected Sprite zeroSprite;

	[SerializeField]
	[HideInInspector]
	protected Sprite halfSprite;

	[SerializeField]
	[HideInInspector]
	protected Sprite oneSprite;

	[SerializeField]
	[HideInInspector]
	protected Color highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1);

	[SerializeField]
	[HideInInspector]
	protected Color pressedColor = new Color(0.75f, 0.75f, 0.75f, 1);

	protected void Start()
	{
		if (buttons == null)
		{
			SpawnButtons();
			RefreshRating();
		}
	}

	protected void SpawnButtons()
	{
		buttons = new Button[maxRating];
		for (int i = 0; i < maxRating; i++)
			SpawnButton(i);
	}

	protected void DestroyButtons()
	{
		Transform transform = this.transform;
		for (int i = transform.childCount - 1; i >= 0; i--)
			transform.GetChild(i).gameObject.Destroy();
	}

	protected void SpawnButton(int index)
	{
		GameObject spriteGO = new GameObject("Button " + (index + 1), typeof(Image));
		Button button = spriteGO.AddComponent<Button>();
		button.transform.SetParent(transform, false);
		button.interactable = !isReadonly;
		
		var colors = button.colors;
		colors.disabledColor = colors.normalColor;
		colors.highlightedColor = highlightedColor;
		colors.pressedColor = pressedColor;
		button.colors = colors;

		button.OnPointerUpAsObservable().Subscribe(OnClick);

		buttons[index] = button;
	}

	protected void OnClick(PointerEventData data)
	{
		if (isReadonly)
			return;

		RectTransform rect = (RectTransform) data.selectedObject.transform;
		int index = rect.GetSiblingIndex();
		if (allowHalf)
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
		newRating = Mathf.Clamp(newRating, 0, maxRating);

		int intPart = (int) newRating;
		float decimalPart = newRating % 1;
		bool half = allowHalf ? decimalPart >= 0.5f : false;

		if (half)
			rating = intPart + 0.5f;
		else
			rating = intPart;

		if (buttons != null && buttons.Length > 0)
		{
			for (int i = 0; i < intPart; i++)
				buttons[i].image.sprite = oneSprite;

			if (intPart < maxRating)
			{
				buttons[intPart].image.sprite = half ? halfSprite : zeroSprite;
				for (int i = intPart + 1; i < maxRating; i++)
					buttons[i].image.sprite = zeroSprite;
			}
		}
	}

	protected void RefreshRating()
	{
		SetRating(rating);
	}

	protected float GetMaxRatingAsFloat()
	{
		return maxRating;
	}

	[ShowInInspector]
	[PropertyRange(1, 10)]
	public int MaxRating
	{
		get
		{
			return maxRating;
		}
		set
		{
			maxRating = value;
			if (Application.isPlaying)
			{
				DestroyButtons();
				SpawnButtons();
			}
			RefreshRating();
		}
	}

	[ShowInInspector]
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

	[ShowInInspector]
	public bool AllowHalf
	{
		get
		{
			return allowHalf;
		}
		set
		{
			allowHalf = value;
			RefreshRating();
		}
	}

	[ShowInInspector]
	public bool IsReadonly
	{
		get
		{
			return isReadonly;
		}
		set
		{
			isReadonly = value;
			if (buttons != null)
			{
				foreach (Button button in buttons)
					button.interactable = !value;
			}
		}
	}

	[ShowInInspector]
	[FoldoutGroup("Sprites")]
	public Sprite ZeroSprite
	{
		get
		{
			return zeroSprite;
		}
		set
		{
			zeroSprite = value;
			RefreshRating();
		}
	}

	[ShowInInspector]
	[ShowIf("allowHalf")]
	[FoldoutGroup("Sprites")]
	public Sprite HalfSprite
	{
		get
		{
			return halfSprite;
		}
		set
		{
			halfSprite = value;
			RefreshRating();
		}
	}

	[ShowInInspector]
	[FoldoutGroup("Sprites")]
	public Sprite OneSprite
	{
		get
		{
			return oneSprite;
		}
		set
		{
			oneSprite = value;
			RefreshRating();
		}
	}

	[ShowInInspector]
	[FoldoutGroup("Appearance")]
	public Color HighlightedColor
	{
		get
		{
			return highlightedColor;
		}
		set
		{
			highlightedColor = value;
			if (buttons != null)
			{
				foreach (Button button in buttons)
				{
					var colors = button.colors;
					colors.highlightedColor = value;
					button.colors = colors;
				}
			}
		}
	}

	[ShowInInspector]
	[FoldoutGroup("Appearance")]
	public Color PressedColor
	{
		get
		{
			return pressedColor;
		}
		set
		{
			pressedColor = value;
			if (buttons != null)
			{
				foreach (Button button in buttons)
				{
					var colors = button.colors;
					colors.pressedColor = value;
					button.colors = colors;
				}
			}
		}
	}
}