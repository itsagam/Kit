#if MODDING
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx.Async;

namespace Modding.Loaders
{
	public class DirectModLoader: ModLoader
	{
		public override Mod LoadMod(string path)
		{
			FileAttributes attributes = File.GetAttributes(path);
			if (!attributes.HasFlag(FileAttributes.Directory))
				return null;

			try
			{
				DirectMod mod = new DirectMod(path);
				if (mod.Load())
					return mod;
			}
			catch (Exception)
			{
			}

			return null;
		}

		public override async UniTask<Mod> LoadModAsync(string path)
		{
			FileAttributes attributes = File.GetAttributes(path);
			if (!attributes.HasFlag(FileAttributes.Directory))
				return null;

			try
			{
				DirectMod mod = new DirectMod(path);
				if (await mod.LoadAsync())
					return mod;
			}
			catch (Exception)
			{
			}

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
			string fullPath = GetFullPath(path);
			return File.ReadAllText(fullPath);
		}

		public override UniTask<string> ReadTextAsync(string path)
		{
			string fullPath = GetFullPath(path);
			using (StreamReader stream = new StreamReader(fullPath))
				return stream.ReadToEndAsync().AsUniTask();
		}

		public override byte[] ReadBytes(string path)
		{
			string fullPath = GetFullPath(path);
			return File.ReadAllBytes(fullPath);
		}

		public override async UniTask<byte[]> ReadBytesAsync(string path)
		{
			string fullPath = GetFullPath(path);
			using (FileStream stream = new FileStream(fullPath, FileMode.Open))
			{
				byte[] data = new byte[stream.Length];
				await stream.ReadAsync(data, 0, (int)stream.Length);
				return data;
			}
		}

		public override IEnumerable<string> FindFiles(string path)
		{
			string fullPath = GetFullPath(path);
			if (File.Exists(fullPath))
				return EnumerableExtensions.Yield(path);

			if (!System.IO.Path.HasExtension(path))
			{
				string fullDir = System.IO.Path.GetDirectoryName(fullPath);
				if (Directory.Exists(fullDir))
				{
					string fullFile = System.IO.Path.GetFileName(fullPath);
					var matching = Directory.EnumerateFiles(fullDir, $"{fullFile}.*");
					string relativeDir = System.IO.Path.GetDirectoryName(path);
					if (matching.Any())
						return matching.Select(p => relativeDir + "/" + System.IO.Path.GetFileName(p));
				}
			}

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