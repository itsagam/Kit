using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Modding.Parsers
{
	public class AudioParser : ResourceParser
	{
		public override List<Type> SupportedTypes => new List<Type> { typeof(AudioClip) };
		public override List<string> SupportedExtensions => new List<string> { ".wav" };
		public override OperateType OperateWith => OperateType.Bytes;

		public override object Read<T>(object data, string path)
		{
			AudioClip clip = WavUtility.ToAudioClip((byte[]) data, 0, path);
			return clip;
		}
	}
}