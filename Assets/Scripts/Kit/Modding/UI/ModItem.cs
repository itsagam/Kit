using DG.Tweening;
using Kit.UI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Kit.Modding.UI
{
	/// <summary>A UI element to display a <see cref="Mod" />'s data and allow configuring it.</summary>
	public class ModItem: Item
	{
		/// <summary>Button to use for enabling/disabling the mod.</summary>
		[Tooltip("Button to use for enabling/disabling the mod.")]
		[SceneObjectsOnly]
		public Toggle EnableToggle;

		/// <summary>Label to use for displaying mod name.</summary>
		[Tooltip("Label to use for displaying mod name.")]
		[SceneObjectsOnly]
		public Text NameText;

		/// <summary>Label to use for displaying mod version.</summary>
		[Tooltip("Label to use for displaying mod version.")]
		[SceneObjectsOnly]
		public Text VersionText;

		/// <summary>Label to use for displaying mod author.</summary>
		[Tooltip("Label to use for displaying mod author.")]
		[SceneObjectsOnly]
		public Text AuthorText;

		/// <summary>Label to use for displaying mod description.</summary>
		[Tooltip("Label to use for displaying mod description.")]
		[SceneObjectsOnly]
		public Text DescriptionText;

		/// <summary>Button to use for moving the mod up in the load order.</summary>
		[Tooltip("Button to use for moving the mod up in the load order.")]
		[SceneObjectsOnly]
		public Button MoveUpButton;

		/// <summary>Button to use for moving the mod down in the load order.</summary>
		[Tooltip("Button to use for moving the mod up in the load order.")]
		[SceneObjectsOnly]
		public Button MoveDownButton;

		/// <summary>Color of the name label when the mod is enabled.</summary>
		[Tooltip("Color of the name label when the mod is enabled.")]
		public Color EnabledColor;

		/// <summary>Color of the name label when the mod is disabled.</summary>
		[Tooltip("Color of the name label when the mod is disabled.")]
		public Color DisabledColor;

		/// <summary>Toggle animation time.</summary>
		[Tooltip("Toggle animation time.")]
		public float RecolorTime = 0.35f;

		/// <summary>Move up/down animation time.</summary>
		[Tooltip("Move up/down animation time.")]
		public float ReorderTime = 0.35f;

#if MODDING
		protected ModWindow window;
		protected new Transform transform;

		protected void Awake()
		{
			window = GetComponentInParent<ModWindow>();
			transform = base.transform;
			EnableToggle.onValueChanged.AddListener(Toggle);
			MoveUpButton.onClick.AddListener(MoveUp);
			MoveDownButton.onClick.AddListener(MoveDown);
		}

		/// <summary>(Re)load the mod data.</summary>
		public override void Refresh()
		{
			EnableToggle.isOn = ModManager.IsModEnabled(Mod);

			var list = ModManager.GetModsByGroup(ModType.Mod);
			if (list[0] == Mod)
				MoveUpButton.SetInteractableImmediate(false);

			if (list[list.Count - 1] == Mod)
				MoveDownButton.SetInteractableImmediate(false);

			ModMetadata metadata = Mod.Metadata;
			NameText.text = metadata.Name;
			NameText.color = EnableToggle.isOn ? EnabledColor : DisabledColor;
			VersionText.text = metadata.Version;
			AuthorText.text = metadata.Author;
			DescriptionText.text = metadata.Description;
		}

		/// <summary>Move the mod up in the load order.</summary>
		public void MoveUp()
		{
			ModManager.MoveModUp(Mod);
			Move(transform.GetSiblingIndex() - 1);
		}

		/// <summary>Move the mod down in the load order.</summary>
		public void MoveDown()
		{
			ModManager.MoveModDown(Mod);
			Move(transform.GetSiblingIndex() + 1);
		}

		/// <summary>Move the mod to a particular index.</summary>
		protected void Move(int toIndex)
		{
			if (window.IsAnimating)
				return;

			window.IsAnimating = true;
			window.IsDirty = true;

			Transform toTransform = transform.parent.GetChild(toIndex);
			int fromIndex = transform.GetSiblingIndex();

			SetInteractable(toIndex);
			toTransform.GetComponent<ModItem>().SetInteractable(fromIndex);

			Sequence sequence = DOTween.Sequence();
			sequence.Insert(0, transform.DOMove(toTransform.position, ReorderTime));
			sequence.Insert(0, toTransform.DOMove(transform.position, ReorderTime));
			sequence.OnComplete(() =>
								{
									transform.SetSiblingIndex(toIndex);
									window.IsAnimating = false;
								});
		}

		protected void SetInteractable(int index)
		{
			MoveUpButton.interactable = index   != 0;
			MoveDownButton.interactable = index < transform.parent.childCount - 1;
		}

		/// <summary>Toggle the mod on or off.</summary>
		public void Toggle(bool value)
		{
			ModManager.ToggleMod(Mod, value);
			NameText.DOColor(value ? EnabledColor : DisabledColor, RecolorTime);
			window.IsDirty = true;
		}

		public Mod Mod => (Mod) Data;
#endif
	}
}