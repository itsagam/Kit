#if MODDING
using System;
using System.Collections.Generic;
using Engine.Parsers;
using UniRx.Async;

namespace Engine.Modding.Loaders
{
	public class AssetBundleMod: Mod
	{
		public override (object reference, string filePath, ResourceParser parser) LoadEx(Type type, string path)
		{
			return base.LoadEx(type, path);
		}

		public override UniTask<(object reference, string filePath, ResourceParser parser)> LoadExAsync(Type type, string path)
		{
			return base.LoadExAsync(type, path);
		}

		public override IEnumerable<string> FindFiles(string path)
		{
			throw new System.NotImplementedException();
		}

		public override bool Exists(string path)
		{
			throw new System.NotImplementedException();
		}

		public override string ReadText(string path)
		{
			throw new System.NotImplementedException();
		}

		public override UniTask<string> ReadTextAsync(string path)
		{
			throw new System.NotImplementedException();
		}

		public override byte[] ReadBytes(string path)
		{
			throw new System.NotImplementedException();
		}

		public override UniTask<byte[]> ReadBytesAsync(string path)
		{
			throw new System.NotImplementedException();
		}
	}
}
#endif