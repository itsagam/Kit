using UnityEngine;
using UnityEngine.UI;

namespace Engine.UI.Behaviours
{
	// Allows to set on/off color of a Toggle
	[RequireComponent(typeof(Toggle))]
	public class ToggleColor : ToggleBehaviour
	{
		public Color OnColor = Color.white;
		public Color OffColor = Color.grey;

		protected override void SetValue(bool value)
		{
			ColorBlock colorBlock = toggle.colors;
			colorBlock.normalColor = colorBlock.highlightedColor = value ? OnColor : OffColor;
			toggle.colors = colorBlock;
		}
	}
}