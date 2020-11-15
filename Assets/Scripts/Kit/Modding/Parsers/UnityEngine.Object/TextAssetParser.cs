using System;
using UnityEngine;

namespace Kit.Parsers
{
	public class TextAssetParser: ResourceParser
	{
		public override Type[] SupportedTypes { get; } = { typeof(TextAsset) };
		public override string[] SupportedExtensions { get; } = { ".txt" };
		public override ParseMode ParseMode => ParseMode.Text;

		public override object Read(Type type, object data, string path = null)
		{
			TextAsset asset = new TextAsset((string) data);
			if (path != null)
				asset.name = path;
			return asset;
		}

		public override object Write(object data, string path = null)
		{
			return ((TextAsset) data).text;
		}
	}
}