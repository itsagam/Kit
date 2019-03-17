using UnityEngine;
using UnityEngine.UI;

namespace Engine
{
	public static class ColorBlockExtensions
	{
		public static ColorBlock SetNormalColor(this ColorBlock colorBlock, Color normalColor)
		{
			colorBlock.normalColor = normalColor;
			return colorBlock;
		}

		public static ColorBlock SetPressedColor(this ColorBlock colorBlock, Color pressedColor)
		{
			colorBlock.pressedColor = pressedColor;
			return colorBlock;
		}

		public static ColorBlock SetHighlightedColor(this ColorBlock colorBlock, Color disabledColor)
		{
			colorBlock.highlightedColor = disabledColor;
			return colorBlock;
		}

		public static ColorBlock SetDisabledColor(this ColorBlock colorBlock, Color disabledColor)
		{
			colorBlock.disabledColor = disabledColor;
			return colorBlock;
		}

		public static ColorBlock SetFadeDuration(this ColorBlock colorBlock, float fadeDuration)
		{
			colorBlock.fadeDuration = fadeDuration;
			return colorBlock;
		}

		public static ColorBlock SetColorMultiplier(this ColorBlock colorBlock, float colorMultiplier)
		{
			colorBlock.colorMultiplier = colorMultiplier;
			return colorBlock;
		}
	}
}