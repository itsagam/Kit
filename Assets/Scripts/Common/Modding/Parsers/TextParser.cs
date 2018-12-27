using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Modding.Parsers
{
	public class TextParser : ModParser
	{
		public override ParseType ParseWith => ParseType.Text;
		public override List<Type> SupportedTypes => new List<Type> { typeof(TextAsset) };

		public override T Parse<T>(string path, string data)
		{
			TextAsset asset = new TextAsset(data);
			asset.name = path;
			return asset as T;
		}
	}
}