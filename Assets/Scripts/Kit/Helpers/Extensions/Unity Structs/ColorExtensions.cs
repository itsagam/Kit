using UnityEngine;

namespace Kit
{
	/// <summary><see cref="Color" /> extensions.</summary>
	public static class ColorExtensions
	{
		/// <summary>Copy values from another instance and return the <see cref="Color" />.</summary>
		public static Color Copy(this Color color)
		{
			return new Color(color.r, color.g, color.b, color.a);
		}

		/// <summary>Copy red component and return the <see cref="Color" />.</summary>
		public static Color CopyRed(this Color color, Color from)
		{
			color.r = from.r;
			return color;
		}

		/// <summary>Copy green component and return the <see cref="Color" />.</summary>
		public static Color CopyGreen(this Color color, Color from)
		{
			color.g = from.g;
			return color;
		}

		/// <summary>Copy blue component and return the <see cref="Color" />.</summary>
		public static Color CopyBlue(this Color color, Color from)
		{
			color.b = from.b;
			return color;
		}

		/// <summary>Copy alpha component and return the <see cref="Color" />.</summary>
		public static Color CopyAlpha(this Color color, Color from)
		{
			color.a = from.a;
			return color;
		}

		/// <summary>Set red component and return the <see cref="Color" />.</summary>
		public static Color SetRed(this Color color, float r)
		{
			color.r = r;
			return color;
		}

		/// <summary>Set green component and return the <see cref="Color" />.</summary>
		public static Color SetGreen(this Color color, float g)
		{
			color.g = g;
			return color;
		}

		/// <summary>Set blue component and return the <see cref="Color" />.</summary>
		public static Color SetBlue(this Color color, float b)
		{
			color.b = b;
			return color;
		}

		/// <summary>Set alpha component and return the <see cref="Color" />.</summary>
		public static Color SetAlpha(this Color color, float alpha)
		{
			color.a = alpha;
			return color;
		}
	}
}