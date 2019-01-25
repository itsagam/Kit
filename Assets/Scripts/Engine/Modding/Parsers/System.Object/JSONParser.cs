using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Modding.Parsers
{
	public class JSONParser : ResourceParser
	{
		public override List<Type> SupportedTypes => new List<Type> { };
		public override List<string> SupportedExtensions => new List<string> { ".json" };
		public override OperateType OperateWith => OperateType.Text;

		public override object Read<T>(object data, string path = null)
		{
			return FromJson<T>((string) data);
		}

		public override object Write(object data, string path = null)
		{
			return ToJson(data);
		}

		public override void Merge<T>(object current, object overwrite)
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

		// TODO: Use JSON.NET
		public static void OverwriteJson(object data, string overwrite)
		{
			JsonUtility.FromJsonOverwrite(overwrite, data);
		}
	}
}