using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Modding.Parsers
{
	public class TextParser : ResourceParser
	{
		public override List<Type> SupportedTypes => new List<Type> { typeof(TextAsset) };
		public override List<string> SupportedExtensions => new List<string> { ".txt" };
		public override OperateType OperateWith => OperateType.Text;

		public override object Read<T>(object data, string path)
		{
			TextAsset asset = new TextAsset((string) data);
			asset.name = path;
			return asset;
		}
	}
}