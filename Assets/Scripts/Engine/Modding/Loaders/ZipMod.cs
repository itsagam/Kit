#if MODDING
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UniRx.Async;

namespace Modding.Loaders
{
	public class ZipModLoader : ModLoader
	{
		public readonly List<string> SupportedExtensions = new List<string> { ".zip" };

		public override Mod LoadMod(string path)
		{
			FileAttributes attributes = File.GetAttributes(path);
			if (attributes.HasFlag(FileAttributes.Directory))
				return null;

			string fileExtension = Path.GetExtension(path);
			if (!SupportedExtensions.Contains(fileExtension))
				return null;

			try
			{
				ZipMod mod = new ZipMod(path);
				if (mod.LoadMetadata())
					return mod;
			}
			catch
			{
			}

			return null;
		}

		public override async UniTask<Mod> LoadModAsync(string path)
		{
			FileAttributes attributes = File.GetAttributes(path);
			if (attributes.HasFlag(FileAttributes.Directory))
				return null;

			string fileExtension = Path.GetExtension(path);
			if (!SupportedExtensions.Contains(fileExtension))
				return null;

			try
			{
				ZipMod mod = new ZipMod(path);
				if (await mod.LoadMetadataAsync())
					return mod;
			}
			catch
			{
			}

			return null;
		}
	}

	public class ZipMod : Mod
	{
		public FileStream Stream { get; }
		public ZipArchive Archive { get; }

		public ZipMod(string path)
		{
			Path = path;
			Stream = new FileStream(path, FileMode.Open);
			Archive = new ZipArchive(Stream, ZipArchiveMode.Read);
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
					byte[] data = new byte[entry.Length];
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
					byte[] data = new byte[entry.Length];
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

			if (System.IO.Path.HasExtension(path))
				return null;

			// Name is empty string for directory ZipArchiveEntries and we strip the extension of files to compare with our own
			var matching = Archive.Entries.Where(e => e.Name != "" && path.Equals(System.IO.Path.ChangeExtension(e.FullName, null), StringComparison.OrdinalIgnoreCase));
			if (matching.Any())
				return matching.Select(e => e.FullName);

			return null;
		}

		public override void Unload()
		{
			base.Unload();
			Archive.Dispose();
			Stream.Dispose();
		}
	}
}
#endif