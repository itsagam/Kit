using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Modding.Resource.Loaders
{
	public class AudioResourceLoader : ResourceLoader
	{
		public override OperateType OperateWith => OperateType.Bytes;
		public override List<Type> SupportedTypes => new List<Type> { typeof(AudioClip) };

		public override T Load<T>(string path, object data)
		{
			AudioClip clip = WavUtility.ToAudioClip((byte[]) data, 0, path);
			clip.name = path;
			return clip as T;
		}
	}
}