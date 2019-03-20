using System.Text;

namespace Engine
{
	public static class StringBuilderExtensions
	{
		public static int IndexOf(this StringBuilder sb, char value, int startIndex)
		{
			for (int i = startIndex; i < sb.Length; i++)
				if (sb[i] == value)
					return i;
			return -1;
		}

		public static int IndexOf(this StringBuilder sb, string value, int startIndex)
		{
			int length = value.Length;
			int maxSearchLength = sb.Length - length + 1;

			for (int i = startIndex; i < maxSearchLength; ++i)
				if (sb[i] == value[0])
				{
					int index = 1;
					while (index < length && sb[i + index] == value[index])
						++index;

					if (index == length)
						return i;
				}

			return -1;
		}
	}
}