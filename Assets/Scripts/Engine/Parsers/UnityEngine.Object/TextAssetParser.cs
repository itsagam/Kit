using System;
using System.Collections.Generic;
using UnityEngine;

namespace Engine.Parsers
{
	public class TextAssetParser : ResourceParser
	{
		public override IEnumerable<Type> SupportedTypes
		{
			get
			{
				yield return typeof(TextAsset);
			}
		}
		public override IEnumerable<string> SupportedExtensions
		{
			get
			{
				yield return ".txt";
			}
		}
		public override ParseMode ParseMode => ParseMode.Text;

		public override object Read(Type type, object data, string path = null)
		{
			TextAsset asset = new TextAsset((string) data);
			if (path != null)
				asset.name = path;
			return asset;
		}
	}
}