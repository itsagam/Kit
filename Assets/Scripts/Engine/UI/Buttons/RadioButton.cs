using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Engine.UI.Buttons
{
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

			var siblings = transform.parent.GetComponentsInChildren<RadioButton>();
			foreach (Button button in siblings.Select(sibling => sibling.Button))
			{
				block = button.colors;
				block.normalColor = block.highlightedColor = offColor;
				button.colors = block;
			}
			block = Button.colors;
			block.normalColor = block.highlightedColor = onColor;
			Button.colors = block;
		}
	}
}