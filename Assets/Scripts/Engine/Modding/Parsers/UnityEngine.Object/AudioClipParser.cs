using System;
using UnityEngine;

// TODO: Use UnityWebRequest to decode audio data
// Solutions:	Derive DownloadHandlerAudioClip and override GetData method
//				(https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/UnityWebRequestAudio/Public/DownloadHandlerAudio.bindings.cs)
//				Derive WWW, change to a custom Web Request and use GetAudioClip() to decode
//				(https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/UnityWebRequestWWW/Public/WWW.cs)
//				Derive UnityWebRequest and find a way to use custom data
//				(https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/UnityWebRequest/Public/UnityWebRequest.bindings.cs)
// Problems:	DownloadHandlerAudioClip is sealed, so you can't derive from it
//				WWW._uwr is private so you cannot change to a custom web request with custom download handler
//				All data downloading and posting is done internally from C++ and passed to DownloadHandler.ReceiveData which is protected

namespace Engine.Parsers
{
	public class AudioClipParser: ResourceParser
	{
		public override Type[] SupportedTypes { get; } = { typeof(AudioClip) };
		public override string[] SupportedExtensions { get; } = { ".wav" };
		public override ParseMode ParseMode => ParseMode.Binary;

		public override object Read(Type type, object data, string path = null)
		{
			return WavUtility.ToAudioClip((byte[]) data, 0, path);
		}
	}
}