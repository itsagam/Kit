using UnityEngine;
using UnityEngine.UI;

namespace Engine
{
	public static class ColorBlockExtensions
	{
		/// <summary>
		/// Set the Normal color and return the ColorBlock.
		/// </summary>
		public static ColorBlock SetNormalColor(this ColorBlock colorBlock, Color normalColor)
		{
			colorBlock.normalColor = normalColor;
			return colorBlock;
		}

		/// <summary>
		/// Set the Pressed color and return the ColorBlock.
		/// </summary>
		public static ColorBlock SetPressedColor(this ColorBlock colorBlock, Color pressedColor)
		{
			colorBlock.pressedColor = pressedColor;
			return colorBlock;
		}

		/// <summary>
		/// Set the Highlighted color and return the ColorBlock.
		/// </summary>
		public static ColorBlock SetHighlightedColor(this ColorBlock colorBlock, Color disabledColor)
		{
			colorBlock.highlightedColor = disabledColor;
			return colorBlock;
		}

		/// <summary>
		/// Set the Disabled color and return the ColorBlock.
		/// </summary>
		public static ColorBlock SetDisabledColor(this ColorBlock colorBlock, Color disabledColor)
		{
			colorBlock.disabledColor = disabledColor;
			return colorBlock;
		}

		/// <summary>
		/// Set the fade duration and return the ColorBlock.
		/// </summary>
		public static ColorBlock SetFadeDuration(this ColorBlock colorBlock, float fadeDuration)
		{
			colorBlock.fadeDuration = fadeDuration;
			return colorBlock;
		}

		/// <summary>
		/// Set the color multiplier and return the ColorBlock.
		/// </summary>
		public static ColorBlock SetColorMultiplier(this ColorBlock colorBlock, float colorMultiplier)
		{
			colorBlock.colorMultiplier = colorMultiplier;
			return colorBlock;
		}
	}
}