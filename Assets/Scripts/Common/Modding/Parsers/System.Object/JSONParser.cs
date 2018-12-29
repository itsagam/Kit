using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

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

		public override object Write(object obj, string path = null)
		{
			return ToJson(obj);
		}

		public override void Merge<T>(T data, T overwrite)
		{
			OverwriteJson(data, ToJson(overwrite));
		}

		public virtual T FromJson<T>(string json)
		{
			return JsonUtility.FromJson<T>(json);
		}

		public virtual string ToJson(object data)
		{
			return JsonUtility.ToJson(data, true);
		}

		public virtual void OverwriteJson(object data, string overwrite)
		{
			JsonUtility.FromJsonOverwrite(overwrite, data);
		}
	}
}