using DG.Tweening;
using Engine;
using Engine.UI;
using Engine.UI.Widgets;
using UnityEngine;
using UnityEngine.UI;
#if MODDING
using Engine.Modding;
#endif

namespace Game.UI.Modding
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

		protected Transform transformCached;

#if MODDING
		protected void Awake()
		{
			transformCached = transform;
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
			Move(transformCached.GetSiblingIndex() - 1);
		}

		protected void MoveDown()
		{
			ModManager.MoveModDown(Mod);
			Move(transformCached.GetSiblingIndex() + 1);
		}

		protected void Move(int toIndex)
		{
			Transform toTransform = transformCached.parent.GetChild(toIndex);
			int fromIndex = transformCached.GetSiblingIndex();

			Sequence sequence = DOTween.Sequence();
			sequence.Insert(0, transformCached.DOMove(toTransform.position, ReorderTime));
			sequence.Insert(0, toTransform.DOMove(transformCached.position, ReorderTime));
			sequence.InsertCallback(ReorderTime / 2.0f,
									() =>
									{
										SetInteractable(toIndex);
										toTransform.GetComponent<ModIcon>().SetInteractable(fromIndex);
									});
			sequence.OnComplete(() => transformCached.SetSiblingIndex(toIndex));
			ModWindow.SetDirty();
		}

		public void SetInteractable(int index)
		{
			MoveUpButton.SetInteractableImmediate(index != 0);
			MoveDownButton.SetInteractableImmediate(index < transformCached.parent.childCount - 1);
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