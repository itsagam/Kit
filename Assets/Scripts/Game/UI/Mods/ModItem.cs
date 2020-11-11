using DG.Tweening;
using Engine;
using Engine.UI;
using UnityEngine;
using UnityEngine.UI;

#if MODDING
using Engine.Modding;
#endif
namespace Game.UI.Mods
{
	public class ModItem: Item
	{
		public Toggle EnableToggle;

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

		public void SetInteractable(int index)
		{
			MoveUpButton.interactable = index != 0;
			MoveDownButton.interactable = index < transform.parent.childCount - 1;
		}

		protected void Toggle(bool value)
		{
			ModManager.ToggleMod(Mod, value);
			NameText.DOColor(value ? EnabledColor : DisabledColor, RecolorTime);
			window.IsDirty = true;
		}

		public Mod Mod => (Mod) Data;
#endif
	}
}