using UnityEngine;
using UnityEngine.UI;

namespace Kit.UI.Behaviours
{
	/// <summary>
	/// Allows to set the on/off color of a <see cref="UnityEngine.UI.Toggle" />.
	/// </summary>
	[RequireComponent(typeof(Toggle))]
	public class ToggleColor: ToggleBehaviour
	{
		/// <summary>
		/// Color to use for the On state.
		/// </summary>
		[Tooltip("Color to use for the On state.")]
		public Color OnColor = Color.white;

		/// <summary>
		/// Color to use for the Off state.
		/// </summary>
		[Tooltip("Color to use for the Off state.")]
		public Color OffColor = Color.grey;

		protected override void OnValueChanged(bool value)
		{
			ColorBlock colorBlock = toggle.colors;
			colorBlock.normalColor = colorBlock.highlightedColor = value ? OnColor : OffColor;
			toggle.colors = colorBlock;
		}
	}
}