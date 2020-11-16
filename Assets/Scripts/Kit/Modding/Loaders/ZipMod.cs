#if MODDING
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace Kit.Modding.Loaders
{
	public class ZipModLoader: ModLoader
	{
		public readonly List<string> SupportedExtensions = new List<string> { ".zip" };

		public override Mod LoadMod(string path)
		{
			FileAttributes attributes = File.GetAttributes(path);
			if (attributes.HasFlag(FileAttributes.Directory))
				return null;

			if (!ResourceManager.MatchExtension(path, SupportedExtensions))
				return null;

			ZipArchive archive = null;
			try
			{
				archive = ZipFile.OpenRead(path);
				ZipMod mod = new ZipMod(archive);
				ModMetadata metadata = mod.Load<ModMetadata>(MetadataFile);
				if (metadata == null)
				{
					Debugger.Log("ModManager", $"Could not load metadata for mod \"{path}\"");
					return null;
				}

				mod.Metadata = metadata;
				return mod;
			}
			catch (Exception ex)
			{
				archive?.Dispose();
				Debugger.Log("ModManager", $"Error loading mod \"{path}\" – {ex.Message}");
				return null;
			}
		}

		public override async UniTask<Mod> LoadModAsync(string path)
		{
			FileAttributes attributes = File.GetAttributes(path);
			if (attributes.HasFlag(FileAttributes.Directory))
				return null;

			if (!ResourceManager.MatchExtension(path, SupportedExtensions))
				return null;

			ZipArchive archive = null;
			try
			{
				archive = ZipFile.OpenRead(path);
				ZipMod mod = new ZipMod(archive);
				ModMetadata metadata = await mod.LoadAsync<ModMetadata>(MetadataFile);
				if (metadata == null)
				{
					Debugger.Log("ModManager", $"Could not load metadata for mod \"{path}\"");
					return null;
				}

				mod.Metadata = metadata;
				return mod;
			}
			catch (Exception ex)
			{
				archive?.Dispose();
				Debugger.Log("ModManager", $"Error loading mod \"{path}\" – {ex.Message}");
				return null;
			}
		}
	}

	public class ZipMod: Mod
	{
		public ZipArchive Archive { get; }

		public ZipMod(ZipArchive archive)
		{
			Archive = archive;
		}

		public override bool Exists(string path)
		{
			return Archive.GetEntry(path) != null;
		}

		public override string ReadText(string path)
		{
			try
			{
				ZipArchiveEntry entry = Archive.GetEntry(path);
				using (Stream stream = entry.Open())
				using (TextReader text = new StreamReader(stream))
					return text.ReadToEnd();
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
				ZipArchiveEntry entry = Archive.GetEntry(path);
				using (Stream stream = entry.Open())
				using (TextReader text = new StreamReader(stream))
					return await text.ReadToEndAsync();
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
				ZipArchiveEntry entry = Archive.GetEntry(path);
				using (Stream stream = entry.Open())
				{
					var data = new byte[entry.Length];
					stream.Read(data, 0, (int) entry.Length);
					return data;
				}
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
				ZipArchiveEntry entry = Archive.GetEntry(path);
				using (Stream stream = entry.Open())
				{
					var data = new byte[entry.Length];
					await stream.ReadAsync(data, 0, (int) entry.Length);
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
			ZipArchiveEntry result = Archive.GetEntry(path);
			if (result != null)
				return EnumerableExtensions.One(path);

			if (Path.HasExtension(path))
				return Enumerable.Empty<string>();

			// Name is empty string for directory ZipArchiveEntries
			return Archive.Entries
						  .Where(entry => entry.Name != "" &&
										  ResourceManager.ComparePath(path, Path.ChangeExtension(entry.FullName, null)))
						  .Select(entry => entry.FullName);
		}

		public override void Unload()
		{
			base.Unload();
			Archive.Dispose();
		}
	}
}
#endif