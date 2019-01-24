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
        public string MetadataFile;

        public DirectModLoader(string metadataFile = "Metadata.json")
        {
            MetadataFile = metadataFile;
        }

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
			string matching = FindFileEx(path, false, true);
			if (matching == null)
				throw GetNotFoundException(path);

			return File.ReadAllText(matching);
		}

		public override async UniTask<string> ReadTextAsync(string path)
		{
			string matching = FindFileEx(path, false, true);
			if (matching == null)
				throw GetNotFoundException(path);

			using (StreamReader stream = new StreamReader(matching))
				return await stream.ReadToEndAsync();
		}

		public override byte[] ReadBytes(string path)
		{
			string matching = FindFileEx(path, false, true);
			if (matching == null)
				throw GetNotFoundException(path);

			return File.ReadAllBytes(matching);
		}

		public override async UniTask<byte[]> ReadBytesAsync(string path)
		{
			string matching = FindFileEx(path, false, true);
			if (matching == null)
				throw GetNotFoundException(path);

			byte[] content;
			using (FileStream stream = new FileStream(matching, FileMode.Open))
			{
				content = new byte[stream.Length];
				await stream.ReadAsync(content, 0, (int)stream.Length);
			}
			return content;
		}

		public override IEnumerable<string> FindFiles(string path)
		{
			return FindFilesEx(path, false, false);
		}

		public virtual string FindFileEx(string path, bool inFullPath, bool outFullPath)
		{
			return FindFilesEx(path, inFullPath, outFullPath).FirstOrDefault();
		}

		// Will not work for inFullPath = true, outFullPath = false
		public virtual IEnumerable<string> FindFilesEx(string path, bool inFullPath, bool outFullPath)
		{
			string fullPath = inFullPath ? path : GetFullPath(path);
			if (File.Exists(fullPath))
				return outFullPath ? fullPath.Yield() : path.Yield();

			if (!System.IO.Path.HasExtension(fullPath))
			{
				string fullDir = System.IO.Path.GetDirectoryName(fullPath);
				if (Directory.Exists(fullDir))
				{
					string fullFile = System.IO.Path.GetFileName(fullPath);
					var matching = Directory.EnumerateFiles(fullDir, $"{fullFile}.*");
					if (matching.Any())
						return outFullPath ? matching : matching.Select(p => path + System.IO.Path.GetExtension(p));
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