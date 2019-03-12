using DG.Tweening;
using Engine.UI.Widgets;
using UnityEngine;
using UnityEngine.UI;
#if MODDING
using Engine.Modding;
#endif

namespace Engine.UI.Modding
{
	public class ModIcon : Icon
	{
		public ToggleButton EnableToggle;

		public Text NameText;
		public Text VersionText;
		public Text AuthorText;
		public Text DescriptionText;
		public Button MoveUpButton;
		public Button MoveDownButton;

		public Color EnabledColor;
		public Color DisabledColor;
		public float RecolorTime = 0.35f;
		public float ReorderTime = 0.35f;

#if MODDING
		protected void Awake()
		{
			EnableToggle.OnValueChanged.AddListener(Toggle);
			MoveUpButton.onClick.AddListener(MoveUp);
			MoveDownButton.onClick.AddListener(MoveDown);
		}

		public override void Refresh()
		{
			EnableToggle.IsOn = ModManager.IsModEnabled(Mod);

			var list = ModManager.GetModsByGroup(ModType.Mod);
			if (list[0] == Mod)
				MoveUpButton.SetInteractableImmediate(false);

			if (list[list.Count - 1] == Mod)
				MoveDownButton.SetInteractableImmediate(false);

			ModMetadata metadata = Mod.Metadata;
			NameText.text = metadata.Name;
			NameText.color = EnableToggle.IsOn ? EnabledColor : DisabledColor;
			VersionText.text = metadata.Version;
			AuthorText.text = metadata.Author;
			DescriptionText.text = metadata.Description;
		}

		protected void MoveUp()
		{
			ModManager.MoveModUp(Mod);
			Move(transform.GetSiblingIndex() - 1);
		}

		protected void MoveDown()
		{
			ModManager.MoveModDown(Mod);
			Move(transform.GetSiblingIndex() + 1);
		}

		protected void Move(int toIndex)
		{
			Transform toTransform = transform.parent.GetChild(toIndex);

			transform.SetSiblingIndex(toIndex);
			Sequence sequence = DOTween.Sequence();
			sequence.Insert(0, transform.DOMove(toTransform.position, ReorderTime));
			sequence.Insert(0, toTransform.DOMove(transform.position, ReorderTime));
			sequence.InsertCallback(ReorderTime / 2.0f, () =>
														{
															RefreshInteractable();
															toTransform.GetComponent<ModIcon>().RefreshInteractable();
														});
			ModWindow.SetDirty();
		}

		public void RefreshInteractable()
		{
			MoveUpButton.SetInteractableImmediate(!transform.IsFirstSibling());
			MoveDownButton.SetInteractableImmediate(!transform.IsLastSibling());
		}

		protected void Toggle(bool value)
		{
			ModManager.ToggleMod(Mod, value);
			NameText.DOColor(value ? EnabledColor : DisabledColor, RecolorTime);
			ModWindow.SetDirty();
		}

		public Mod Mod => (Mod) Data;
#endif
	}
}