using UnityEngine;
using System;
using System.Collections.Generic;

public class Texture2DParser : ResourceParser
{
	public override IEnumerable<Type> SupportedTypes
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
	public override ParseMode ParseMode => ParseMode.Binary;

	public override object Read(Type type, object data, string path = null)
	{
		Texture2D texture = new Texture2D(0, 0);
		texture.LoadImage((byte[]) data);
		if (path != null)
			texture.name = path;
		return texture;
	}
}