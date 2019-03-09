using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Modding.Parsers
{
	public class JSONParser : ResourceParser
	{
		public override IEnumerable<Type> SupportedTypes => Enumerable.Empty<Type>();
		public override IEnumerable<string> SupportedExtensions
		{
			get
			{
				yield return ".json";
			}
		}
		public override OperateType OperateWith => OperateType.Text;

		public override object Read(Type type, object data, string path = null)
		{
			return JsonConvert.DeserializeObject((string) data, type);
		}

		public override object Write(object data, string path = null)
		{
			return ToJson(data);
		}

		public override void Merge(object current, object overwrite)
		{
			OverwriteJson(current, (string) overwrite);
		}

		public static T FromJson<T>(string json)
		{
			return JsonConvert.DeserializeObject<T>(json);
		}

		public static string ToJson(object data)
		{
			return JsonConvert.SerializeObject(data);
		}
		
		public static void OverwriteJson(object data, string overwrite)
		{
			JsonConvert.PopulateObject(overwrite, data);
		}
	}
}