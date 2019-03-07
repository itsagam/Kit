using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Modding.Parsers
{
	public class TextParser : ResourceParser
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
		public override OperateType OperateWith => OperateType.Text;

		public override object Read(Type type, object data, string path)
		{
			TextAsset asset = new TextAsset((string) data);
			asset.name = path;
			return asset;
		}
	}
}