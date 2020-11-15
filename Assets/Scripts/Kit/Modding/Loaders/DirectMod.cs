#if MODDING
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace Kit.Modding.Loaders
{
	public class DirectModLoader: ModLoader
	{
		public override Mod LoadMod(string path)
		{
			FileAttributes attributes = File.GetAttributes(path);
			if (!attributes.HasFlag(FileAttributes.Directory))
				return null;

			DirectMod mod = new DirectMod(path);
			ModMetadata metadata = mod.Load<ModMetadata>(MetadataFile);
			if (metadata == null)
			{
				Debugger.Log("ModManager", $"Could not load metadata for mod \"{path}\"");
				return null;
			}

			mod.Metadata = metadata;
			return mod;
		}

		public override async UniTask<Mod> LoadModAsync(string path)
		{
			FileAttributes attributes = File.GetAttributes(path);
			if (!attributes.HasFlag(FileAttributes.Directory))
				return null;

			DirectMod mod = new DirectMod(path);
			ModMetadata metadata = await mod.LoadAsync<ModMetadata>(MetadataFile);
			if (metadata == null)
			{
				Debugger.Log("ModManager", $"Could not load metadata for mod \"{path}\"");
				return null;
			}

			mod.Metadata = metadata;
			return mod;
		}
	}

	public class DirectMod: Mod
	{
		public string Path { get; }

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
				return Enumerable.Empty<string>();

			// There are three ways to match files without an extension:
			// 1) Extract the extension of found files and strap it to input path
			// 2) Extract the directory of input path, and strap it to found filenames
			// 3) Remove the absolute path part from found file paths
			// Only the third can correct typos in input path, but since we yield input path if it exists anyway...

			try
			{
				string fullDir = System.IO.Path.GetDirectoryName(fullPath);
				string fullFile = System.IO.Path.GetFileName(fullPath);
				var foundPaths = Directory.EnumerateFiles(fullDir, $"{fullFile}.*");
				return foundPaths.Select(p => path + System.IO.Path.GetExtension(p));
			}
			catch
			{
				return Enumerable.Empty<string>();
			}
		}

		public override bool Exists(string path)
		{
			return File.Exists(GetFullPath(path));
		}

		public string GetFullPath(string subPath)
		{
			return Path + subPath;
		}
	}
}
#endif