﻿#if MODDING
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniRx.Async;

namespace Engine.Modding.Loaders
{
	public class DirectModLoader: ModLoader
	{
		public override Mod LoadMod(string path)
		{
			FileAttributes attributes = File.GetAttributes(path);
			if (!attributes.HasFlag(FileAttributes.Directory))
				return null;

			DirectMod mod = new DirectMod(path);
			if (mod.LoadMetadata())
				return mod;

			return null;
		}

		public override async UniTask<Mod> LoadModAsync(string path)
		{
			FileAttributes attributes = File.GetAttributes(path);
			if (!attributes.HasFlag(FileAttributes.Directory))
				return null;

			DirectMod mod = new DirectMod(path);
			if (await mod.LoadMetadataAsync())
				return mod;

			return null;
		}
	}

	public class DirectMod: Mod
	{
		public DirectMod(string path)
		{
			Path = path + "/";
		}

		public override string ReadText(string path)
		{
			try
			{
				string fullPath = GetFullPath(path);
				return File.ReadAllText(fullPath);
			}
			catch
			{
				return null;
			}
		}

		public override async UniTask<string> ReadTextAsync(string path)
		{
			try
			{
				string fullPath = GetFullPath(path);
				using (StreamReader stream = new StreamReader(fullPath))
					return await stream.ReadToEndAsync();
			}
			catch
			{
				return null;
			}
		}

		public override byte[] ReadBytes(string path)
		{
			try
			{
				string fullPath = GetFullPath(path);
				return File.ReadAllBytes(fullPath);
			}
			catch
			{
				return null;
			}
		}

		public override async UniTask<byte[]> ReadBytesAsync(string path)
		{
			try
			{
				string fullPath = GetFullPath(path);
				using (FileStream stream = new FileStream(fullPath, FileMode.Open))
				{
					byte[] data = new byte[stream.Length];
					await stream.ReadAsync(data, 0, (int)stream.Length);
					return data;
				}
			}
			catch
			{
				return null;
			}
		}

		public override IEnumerable<string> FindFiles(string path)
		{
			string fullPath = GetFullPath(path);
			if (File.Exists(fullPath))
				return EnumerableExtensions.One(path);

			if (System.IO.Path.HasExtension(path))
				return null;

			string fullDir = System.IO.Path.GetDirectoryName(fullPath);
			if (!Directory.Exists(fullDir))
				return null;

			string fullFile = System.IO.Path.GetFileName(fullPath);
			var matching = Directory.EnumerateFiles(fullDir, $"{fullFile}.*");
			string relativeDir = System.IO.Path.GetDirectoryName(path);

			if (matching.Any())
				return matching.Select(p => relativeDir + "/" + System.IO.Path.GetFileName(p));

			return null;
		}

		public override bool Exists(string path)
		{
			return File.Exists(GetFullPath(path));
		}

		public virtual string GetFullPath(string subPath)
		{
			return Path + subPath;
		}
	}
}
#endif