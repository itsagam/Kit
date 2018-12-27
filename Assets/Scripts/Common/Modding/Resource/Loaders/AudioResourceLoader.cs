using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Modding.Resource.Loaders
{
	public class AudioResourceLoader : ResourceLoader
	{
		public override ReadType LoadWith => ReadType.Bytes;
		public override List<Type> SupportedTypes => new List<Type> { typeof(AudioClip) };

		public override T Load<T>(string path, byte[] data)
		{
			return WavUtility.ToAudioClip(data, 0, path) as T;
		}
	}
}