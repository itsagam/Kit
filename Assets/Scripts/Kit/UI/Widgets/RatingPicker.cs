using Sirenix.OdinInspector;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kit.UI.Widgets
{
	/// <summary>
	/// A rating picker/display widget.
	/// </summary>
	[RequireComponent(typeof(HorizontalLayoutGroup))]
	public class RatingPicker: MonoBehaviour
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
			if (buttons != null)
				return;

			SpawnButtons();
			RefreshRating();
		}

		protected void SpawnButtons()
		{
			buttons = new Button[maxRating];
			for (int i = 0; i < maxRating; i++)
				SpawnButton(i);
		}

		protected void DestroyButtons()
		{
			Transform transformCached = transform;
			for (int i = transformCached.childCount - 1; i >= 0; i--)
				transformCached.GetChild(i).gameObject.Destroy();
		}

		protected void SpawnButton(int index)
		{
			GameObject spriteGO = new GameObject("Button " + (index + 1), typeof(Image));
			Button button = spriteGO.AddComponent<Button>();
			button.transform.SetParent(transform, false);
			button.interactable = !isReadonly;

			ColorBlock colors = button.colors;
			colors.disabledColor = colors.normalColor;
			colors.highlightedColor = highlightedColor;
			colors.pressedColor = pressedColor;
			button.colors = colors;

			// OnPointerClickAsObservable doesn't register the selection if the press is released somewhere else
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
				if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,
																			 data.pressPosition,
																			 data.pressEventCamera,
																			 out Vector2 point))
					return;

				float decimalPart = point.x < 0 ? 0.5f : 1.0f;
				SetRating(index + decimalPart);
			}
			else
				SetRating(index + 1);
		}

		protected void SetRating(float newRating)
		{
			newRating = Mathf.Clamp(newRating, 0, maxRating);

			int intPart = (int) newRating;
			float decimalPart = newRating % 1;
			bool half = allowHalf && decimalPart >= 0.5f;

			if (half)
				rating = intPart + 0.5f;
			else
				rating = intPart;

			if (buttons == null || buttons.Length <= 0)
				return;

			for (int i = 0; i < intPart; i++)
				buttons[i].image.sprite = oneSprite;

			if (intPart >= maxRating)
				return;

			buttons[intPart].image.sprite = half ? halfSprite : zeroSprite;
			for (int i = intPart + 1; i < maxRating; i++)
				buttons[i].image.sprite = zeroSprite;
		}

		protected void RefreshRating()
		{
			SetRating(rating);
		}

		/// <summary>
		/// Get or set the maximum rating you can pick or show.
		/// </summary>
		[ShowInInspector]
		[PropertyTooltip("The maximum rating you can pick or show.")]
		[PropertyRange(1, 10)]
		public int MaxRating
		{
			get => maxRating;
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

		/// <summary>
		/// Get or set the current rating.
		/// </summary>
		[ShowInInspector]
		[PropertyTooltip("The current rating.")]
		[PropertyRange(0, "MaxRating")]
		public float Rating
		{
			get => rating;
			set => SetRating(value);
		}

		/// <summary>
		/// Whether to allow half-point ratings.
		/// </summary>
		[ShowInInspector]
		[PropertyTooltip("Whether to allow half-point ratings.")]
		public bool AllowHalf
		{
			get => allowHalf;
			set
			{
				allowHalf = value;
				RefreshRating();
			}
		}

		/// <summary>
		/// Whether to allow to pick a rating or just display it.
		/// </summary>
		[ShowInInspector]
		[PropertyTooltip("Whether to allow to pick a rating or just display it.")]
		public bool IsReadonly
		{
			get => isReadonly;
			set
			{
				isReadonly = value;
				if (buttons == null)
					return;
				foreach (Button button in buttons)
					button.interactable = !value;
			}
		}

		/// <summary>
		/// The <see cref="Sprite"/> to use for a zero point of rating.
		/// </summary>
		[ShowInInspector]
		[FoldoutGroup("Sprites")]
		[PropertyTooltip("The sprite to use for a zero point of rating.")]
		public Sprite ZeroSprite
		{
			get => zeroSprite;
			set
			{
				zeroSprite = value;
				RefreshRating();
			}
		}

		/// <summary>
		/// The <see cref="Sprite"/> to use for half a point of rating.
		/// </summary>
		[ShowInInspector]
		[ShowIf("allowHalf")]
		[FoldoutGroup("Sprites")]
		[PropertyTooltip("The sprite to use for half a point of rating.")]
		public Sprite HalfSprite
		{
			get => halfSprite;
			set
			{
				halfSprite = value;
				RefreshRating();
			}
		}

		/// <summary>
		/// The <see cref="Sprite"/> to use for a full point of rating.
		/// </summary>
		[ShowInInspector]
		[FoldoutGroup("Sprites")]
		[PropertyTooltip("The sprite to use for a full point of rating.")]
		public Sprite OneSprite
		{
			get => oneSprite;
			set
			{
				oneSprite = value;
				RefreshRating();
			}
		}

		/// <summary>
		/// The color of buttons when they're highlighted.
		/// </summary>
		[ShowInInspector]
		[FoldoutGroup("Appearance")]
		[PropertyTooltip("The color of buttons when they're highlighted.")]
		public Color HighlightedColor
		{
			get => highlightedColor;
			set
			{
				highlightedColor = value;
				if (buttons == null)
					return;
				foreach (Button button in buttons)
					button.colors = button.colors.SetHighlightedColor(value);
			}
		}

		/// <summary>
		/// The color of buttons when they're pressed.
		/// </summary>
		[ShowInInspector]
		[FoldoutGroup("Appearance")]
		[PropertyTooltip("The color of buttons when they're pressed.")]
		public Color PressedColor
		{
			get => pressedColor;
			set
			{
				pressedColor = value;
				if (buttons == null)
					return;
				foreach (Button button in buttons)
					button.colors = button.colors.SetPressedColor(value);
			}
		}
	}
}