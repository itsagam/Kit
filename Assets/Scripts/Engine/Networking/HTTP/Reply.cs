using System.Text;

namespace HTTP
{
	public class Reply
	{
		public byte[] Data;

		public Reply(byte[] data = null)
		{
			Data = data;
		}

		public string Text
		{
			get
			{
				return Data != null ? Encoding.UTF8.GetString(Data) : null;
			}
		}

		public override string ToString()
		{
			string output = "{";

			if (Data != null)
				output += $"Text: {Text}";

			output += "}";

			return output;
		}
	}
}