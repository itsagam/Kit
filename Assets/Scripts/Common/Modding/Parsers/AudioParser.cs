using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Modding.Parsers
{
	public class AudioParser : ModParser
	{
		public override ParseType ParseWith => ParseType.Bytes;
		public override List<Type> SupportedTypes => new List<Type> { typeof(AudioClip) };

		public override T Parse<T>(string path, byte[] data)
		{
			return WavUtility.ToAudioClip(data, 0, path) as T;
		}
	}
}