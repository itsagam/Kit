using UnityEngine;
using UnityEngine.UI;

namespace Engine.UI.Buttons
{
	[RequireComponent(typeof(Button))]
	public class PanelRadioButton : RadioButton
	{
		public RectTransform Panel;

		protected override void Awake()
		{
			base.Awake();
			if (Panel != null)
				Panel.gameObject.SetActive(false);
		}

		public override void Select()
		{
			base.Select();
			var siblings = transform.parent.GetComponentsInChildren<PanelRadioButton>();
			foreach (PanelRadioButton sibling in siblings)
				sibling.Panel.gameObject.SetActive(false);
			Panel.gameObject.SetActive(true);
		}
	}
}