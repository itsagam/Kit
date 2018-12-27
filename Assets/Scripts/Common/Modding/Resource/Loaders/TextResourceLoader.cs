using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Modding.Resource.Loaders
{
	public class TextResourceLoader : ResourceLoader
	{
		public override ReadType LoadWith => ReadType.Text;
		public override List<Type> SupportedTypes => new List<Type> { typeof(TextAsset) };

		public override T Load<T>(string path, string data)
		{
			TextAsset asset = new TextAsset(data);
			asset.name = path;
			return asset as T;
		}
	}
}