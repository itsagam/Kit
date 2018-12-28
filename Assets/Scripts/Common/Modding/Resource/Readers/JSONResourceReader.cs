using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Modding.Resource.Readers
{
	public class JSONResourceReader : ResourceReader 
	{
		public override List<string> SupportedExtensions => new List<string> { ".json" };
		public override OperateType OperateWith => OperateType.Text;

		public override T Read<T>(object data)
		{
			return FromJSON<T>((string) data);
		}

		public override object Write(object obj)
		{
			return ToJSON(obj);
		}

		public override void Merge<T>(T data, T overwrite)
		{
			OverwriteJSON(data, ToJSON(overwrite));
		}

		public virtual T FromJSON<T>(string json)
		{
			return JsonUtility.FromJson<T>(json);
		}

		public virtual string ToJSON(object obj)
		{
			return JsonUtility.ToJson(obj, true);
		}

		public virtual void OverwriteJSON(object data, string overwrite)
		{
			JsonUtility.FromJsonOverwrite(overwrite, data);
		}
	}
}