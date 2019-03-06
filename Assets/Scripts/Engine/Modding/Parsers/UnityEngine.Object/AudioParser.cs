using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Modding.Parsers
{
	// Use UnityWebRequest to decode audio data
	// Solutions:	Derive DownloadHandlerAudioClip and override GetData method
	//				(https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/UnityWebRequestAudio/Public/DownloadHandlerAudio.bindings.cs)
	//				Derive WWW, change to a custom Web Request and use GetAudioClip() to decode
	//				(https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/UnityWebRequestWWW/Public/WWW.cs)
	//				Derive UnityWebRequest and find a way to use custom data 
	//				(https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/UnityWebRequest/Public/UnityWebRequest.bindings.cs)
	// Problems:	DownloadHandlerAudioClip is sealed, so you can't derive from it
	//				WWW._uwr is private so you cannot change to a custom web request with custom download handler
	//				All data downloaing and posting is done internally from C++ and passed to DownloadHandler.ReceiveData which is protected

	public class AudioParser : ResourceParser
	{
		public override IEnumerable<Type> SupportedTypes
		{
			get
			{
				yield return typeof(AudioClip);
			}
		}
		public override IEnumerable<string> SupportedExtensions
		{
			get
			{
				yield return ".wav";
			}
		}
		public override OperateType OperateWith => OperateType.Bytes;

		public override T Read<T>(object data, string path)
		{
			return WavUtility.ToAudioClip((byte[]) data, 0, path) as T;
		}
	}
}