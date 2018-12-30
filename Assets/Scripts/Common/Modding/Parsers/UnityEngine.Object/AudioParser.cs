using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Modding.Parsers
{
	// TODO: Use UnityWebRequest to decode audio data
	// Solutions:	Derive DownloadHandlerAudioClip and override GetData method
	//				(https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/UnityWebRequestAudio/Public/DownloadHandlerAudio.bindings.cs)
	//				Derive WWW, change to a custom Web Request and use GetAudioClip() to decode
	//				(https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/UnityWebRequestWWW/Public/WWW.cs)
	//				Derive UnityWebRequest and find a way to use custom data 
	//				(https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/UnityWebRequest/Public/UnityWebRequest.bindings.cs)
	// Problems:	DownloadHandlerAudioClip is sealed, so you can't derive from it
	//				WWW._uwr is private so you cannot change to a custom web request with custom download handler
	//				All data downloaing and posting is done internally from C++ and passed to DownloadHandler.ReceiveData which is protected

	/*
	class DummyAudioRequest : UnityWebRequest
	{
	}

	class DummyDownloadHandler : DownloadHandlerScript
	{
		public byte[] Data { get; protected set; }

		public DummyDownloadHandler(byte[] data)
		{
			Data = data;
		}

		protected override byte[] GetData()
		{
			return Data;
		}
	}

	internal class DummyWWW : WWW
	{
		public DummyWWW(string url) : base(url)
		{			
		}
	}
	*/

	public class AudioParser : ResourceParser
	{
		public override List<Type> SupportedTypes => new List<Type> { typeof(AudioClip) };
		public override List<string> SupportedExtensions => new List<string> { ".wav" };
		public override OperateType OperateWith => OperateType.Bytes;

		public override object Read<T>(object data, string path)
		{
			return WavUtility.ToAudioClip((byte[]) data, 0, path);
		}
	}
}