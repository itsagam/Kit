using System;
using Newtonsoft.Json;

namespace Engine.Parsers
{
	public class JSONParser: ResourceParser
	{
		public override Type[] SupportedTypes { get; } = { };
		public override string[] SupportedExtensions { get; } = { ".json" };
		public override ParseMode ParseMode => ParseMode.Text;

		public override object Read(Type type, object data, string path = null)
		{
			return JsonConvert.DeserializeObject((string) data, type);
		}

		public override object Write(object data, string path = null)
		{
			return JsonConvert.SerializeObject(data, Formatting.Indented);
		}

		public override void Merge(object current, object overwrite)
		{
			JsonConvert.PopulateObject((string) overwrite, current);
		}

		// Static functions
		public static T FromJson<T>(string json)
		{
			return JsonConvert.DeserializeObject<T>(json);
		}

		public static string ToJson(object data)
		{
			return JsonConvert.SerializeObject(data, Formatting.Indented);
		}
	}
}