using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Modding.Parsers
{
	public class TextureParser : ResourceParser
	{
		public override List<Type> SupportedTypes => new List<Type> { typeof(Texture2D) };
		public override List<string> SupportedExtensions => new List<string> { ".jpg", ".jpeg", ".png" };
		public override OperateType OperateWith => OperateType.Bytes;

		public override object Read<T>(object data, string path)
		{
			Texture2D texture = null;
			texture = new Texture2D(0, 0);
			texture.LoadImage((byte[]) data);
			texture.name = path;
			return texture;
		}
	}
}