using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Modding.Parsers
{
	public class TextureParser : ModParser
	{
		public override ParseType ParseWith => ParseType.Bytes;
		public override List<Type> SupportedTypes => new List<Type> { typeof(Texture2D) };

		public override T Parse<T>(string path, byte[] data)
		{
			Texture2D texture = null;
			texture = new Texture2D(0, 0);
			texture.LoadImage(data);
			return texture as T;
		}
	}
}