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
				if (mod.LoadMetadata())
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
				if (await mod.LoadMetadataAsync())
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

		public override async UniTask<string> ReadTextAsync(string path)
		{
			string fullPath = GetFullPath(path);
			using (StreamReader stream = new StreamReader(fullPath))
				return await stream.ReadToEndAsync();
		}

		public override byte[] ReadBytes(string path)
		{
			string fullPath = GetFullPath(path);
			return File.ReadAllBytes(fullPath);
		}

		public override async UniTask<byte[]> ReadBytesAsync(string path)
		{
			string fullPath = GetFullPath(path);
			byte[] content;
			using (FileStream stream = new FileStream(fullPath, FileMode.Open))
			{
				content = new byte[stream.Length];
				await stream.ReadAsync(content, 0, (int)stream.Length);
			}
			return content;
		}

		public override IEnumerable<string> FindFiles(string path)
		{
			string fullPath = GetFullPath(path);
			if (File.Exists(fullPath))
				return path.Yield();

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
						//return matching.Select(p => path + System.IO.Path.GetExtension(p));
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