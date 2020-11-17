using System;
using Newtonsoft.Json;

namespace Kit.Parsers
{
	/// <summary>A Json parser.</summary>
	/// <seealso cref="Kit.Parsers.ResourceParser" />
	public class JsonParser: ResourceParser
	{
		/// <inheritdoc />
		public override Type[] SupportedTypes { get; } = { };

		/// <inheritdoc />
		public override string[] SupportedExtensions { get; } = { ".json" };

		/// <inheritdoc />
		public override ParseMode ParseMode => ParseMode.Text;

		/// <inheritdoc />
		public override object Read(Type type, object data, string path = null)
		{
			return JsonConvert.DeserializeObject((string) data, type);
		}

		/// <inheritdoc />
		public override object Write(object data, string path = null)
		{
			return JsonConvert.SerializeObject(data);
		}

		/// <inheritdoc />
		public override void Merge(object current, object overwrite)
		{
			JsonConvert.PopulateObject((string) overwrite, current);
		}

		// Static functions

		/// <summary>Converts Json to an object.</summary>
		/// <typeparam name="T">Type of the object expected.</typeparam>
		/// <param name="json">The Json to convert.</param>
		public static T FromJson<T>(string json)
		{
			return JsonConvert.DeserializeObject<T>(json);
		}

		/// <summary>Converts an object to Json.</summary>
		/// <param name="data">The object to convert.</param>
		/// <param name="formatted">Whether to return formatted Json.</param>
		public static string ToJson(object data, bool formatted = true)
		{
			return JsonConvert.SerializeObject(data, formatted ? Formatting.Indented : Formatting.None);
		}
	}
}