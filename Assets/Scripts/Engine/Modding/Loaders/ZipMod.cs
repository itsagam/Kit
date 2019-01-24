using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;
using UniRx.Async;

namespace Modding.Loaders
{
	public class ZipModLoader : ModLoader
	{
		public string MetadataFile;

		public ZipModLoader(string metadataFile = "Metadata.json")
		{
			MetadataFile = metadataFile;
		}

		public override Mod LoadMod(string path)
		{
			FileAttributes attributes = File.GetAttributes(path);
			if (attributes.HasFlag(FileAttributes.Directory))
				return null;

			try
			{
				ZipMod mod = new ZipMod(path);
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
			if (attributes.HasFlag(FileAttributes.Directory))
				return null;

			try
			{
				ZipMod mod = new ZipMod(path);
				if (await mod.LoadMetadataAsync())
					return mod;
			}
			catch (Exception)
			{
			}

			return null;
		}
	}

	public class ZipMod : Mod
	{
		public FileStream Stream { get; protected set; }
		public ZipArchive Archive { get; protected set; }

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
			string matching = FindFile(path);
			if (matching == null)
				throw GetNotFoundException(path);

			ZipArchiveEntry entry = Archive.GetEntry(matching);
			Stream stream = entry.Open();
			TextReader text = new StreamReader(stream);
			string data = text.ReadToEnd();
			text.Dispose();
			stream.Dispose();
			return data;
		}

		public override async UniTask<string> ReadTextAsync(string path)
		{
			string matching = FindFile(path);
			if (matching == null)
				throw GetNotFoundException(path);

			ZipArchiveEntry entry = Archive.GetEntry(matching);
			Stream stream = entry.Open();
			TextReader text = new StreamReader(stream);
			string data = await text.ReadToEndAsync();
			text.Dispose();
			stream.Dispose();
			return data;
		}

		public override byte[] ReadBytes(string path)
		{
			string matching = FindFile(path);
			if (matching == null)
				throw GetNotFoundException(path);

			ZipArchiveEntry entry = Archive.GetEntry(matching);
			Stream stream = entry.Open();
			byte[] data = new byte[entry.Length];
			stream.Read(data, 0, (int) entry.Length);
			stream.Dispose();
			return data;
		}

		public override async UniTask<byte[]> ReadBytesAsync(string path)
		{
			string matching = FindFile(path);
			if (matching == null)
				throw GetNotFoundException(path);

			ZipArchiveEntry entry = Archive.GetEntry(matching);
			Stream stream = entry.Open();
			byte[] data = new byte[entry.Length];
			await stream.ReadAsync(data, 0, (int)entry.Length);
			stream.Dispose();
			return data;
		}

		public override IEnumerable<string> FindFiles(string path)
		{
			ZipArchiveEntry result = Archive.GetEntry(path);
			if (result != null)
				return new string[] { path };

			if (! System.IO.Path.HasExtension(path))
			{
				string fileName = System.IO.Path.GetFileName(path);
				string fileDir = System.IO.Path.GetDirectoryName(path);
				
				var matching = Archive.Entries
									.Where(	e => System.IO.Path.GetDirectoryName(e.FullName).Equals(fileDir, StringComparison.OrdinalIgnoreCase) &&
											e.Name != null &&
											System.IO.Path.GetFileNameWithoutExtension(e.Name).Equals(fileName, StringComparison.OrdinalIgnoreCase))
									.Select(e => e.FullName);

				if (matching.Any())
					return matching.ToList();
			}

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