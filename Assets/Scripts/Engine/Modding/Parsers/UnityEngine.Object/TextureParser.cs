﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Modding.Parsers
{
	public class TextureParser : ResourceParser
	{
		public override IEnumerable<Type> SupportedReadTypes
		{
			get
			{
				yield return typeof(Texture2D);
			}
		}
		public override IEnumerable<string> SupportedExtensions
		{
			get
			{
				yield return ".jpg";
				yield return ".jpeg";
				yield return ".png";
			}
		}
		public override OperateType OperateWith => OperateType.Bytes;

		public override T Read<T>(object data, string path)
		{
			Texture2D texture = null;
			texture = new Texture2D(0, 0);
			texture.LoadImage((byte[]) data);
			texture.name = path;
			return (T) (object) texture;
		}
	}
}