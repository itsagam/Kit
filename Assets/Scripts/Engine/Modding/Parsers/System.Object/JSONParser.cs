using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

		public override T Read<T>(object data, string path = null)
		{
			return FromJson<T>((string) data);
		}

		public override object Write<T>(T data, string path = null)
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