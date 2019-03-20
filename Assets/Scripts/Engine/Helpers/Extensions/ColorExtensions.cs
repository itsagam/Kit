using UnityEngine;

namespace Engine
{
	public static class ColorExtensions
	{
		public static Color Copy(this Color color)
		{
			return new Color(color.r, color.g, color.b, color.a);
		}

		public static Color CopyRed(this Color color, Color from)
		{
			color.r = from.r;
			return color;
		}

		public static Color CopyGreen(this Color color, Color from)
		{
			color.g = from.g;
			return color;
		}

		public static Color CopyBlue(this Color color, Color from)
		{
			color.b = from.b;
			return color;
		}

		public static Color CopyAlpha(this Color color, Color from)
		{
			color.a = from.a;
			return color;
		}

		public static Color SetRed(this Color color, float r)
		{
			color.r = r;
			return color;
		}

		public static Color SetGreen(this Color color, float g)
		{
			color.g = g;
			return color;
		}

		public static Color SetBlue(this Color color, float b)
		{
			color.b = b;
			return color;
		}

		public static Color SetAlpha(this Color color, float alpha)
		{
			color.a = alpha;
			return color;
		}
	}
}