using UnityEngine;
using UnityEngine.UI;

namespace Engine.UI.Behaviours
{
	/// <summary>
	/// Allows to set the on/off color of a <see cref="UnityEngine.UI.Toggle" />.
	/// </summary>
	[RequireComponent(typeof(Toggle))]
	public class ToggleColor: ToggleBehaviour
	{
		public Color OnColor = Color.white;
		public Color OffColor = Color.grey;

		protected override void OnValueChanged(bool value)
		{
			ColorBlock colorBlock = toggle.colors;
			colorBlock.normalColor = colorBlock.highlightedColor = value ? OnColor : OffColor;
			toggle.colors = colorBlock;
		}
	}
}