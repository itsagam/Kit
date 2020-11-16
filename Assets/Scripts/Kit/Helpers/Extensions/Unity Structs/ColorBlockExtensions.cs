using UnityEngine;
using UnityEngine.UI;

namespace Kit
{
	/// <summary><see cref="ColorBlock" /> extensions.</summary>
	public static class ColorBlockExtensions
	{
		/// <summary>Set the <see cref="ColorBlock.normalColor" /> and return the <see cref="ColorBlock" />.</summary>
		public static ColorBlock SetNormalColor(this ColorBlock colorBlock, Color normalColor)
		{
			colorBlock.normalColor = normalColor;
			return colorBlock;
		}

		/// <summary>Set the <see cref="ColorBlock.pressedColor" />  and return the <see cref="ColorBlock" />.</summary>
		public static ColorBlock SetPressedColor(this ColorBlock colorBlock, Color pressedColor)
		{
			colorBlock.pressedColor = pressedColor;
			return colorBlock;
		}

		/// <summary>Set the <see cref="ColorBlock.highlightedColor" /> and return the <see cref="ColorBlock" />.</summary>
		public static ColorBlock SetHighlightedColor(this ColorBlock colorBlock, Color disabledColor)
		{
			colorBlock.highlightedColor = disabledColor;
			return colorBlock;
		}

		/// <summary>Set the <see cref="ColorBlock.disabledColor" /> and return the <see cref="ColorBlock" />.</summary>
		public static ColorBlock SetDisabledColor(this ColorBlock colorBlock, Color disabledColor)
		{
			colorBlock.disabledColor = disabledColor;
			return colorBlock;
		}

		/// <summary>Set the fade duration and return the <see cref="ColorBlock" />.</summary>
		public static ColorBlock SetFadeDuration(this ColorBlock colorBlock, float fadeDuration)
		{
			colorBlock.fadeDuration = fadeDuration;
			return colorBlock;
		}

		/// <summary>Set the color multiplier and return the <see cref="ColorBlock" />.</summary>
		public static ColorBlock SetColorMultiplier(this ColorBlock colorBlock, float colorMultiplier)
		{
			colorBlock.colorMultiplier = colorMultiplier;
			return colorBlock;
		}
	}
}