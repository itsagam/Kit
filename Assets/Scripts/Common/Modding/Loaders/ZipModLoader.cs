using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using Modding.Resource.Readers;

namespace Modding.Loaders
{
	public class ZipModLoader : ModLoader
	{
		public string MetadataFile;

		public ZipModLoader(string metadataFile = "Metadata.json")
		{
			MetadataFile = metadataFile;
		}

		protected override async Task<ModPackage> LoadModInternal(string path, bool async)
		{
			FileAttributes attributes = File.GetAttributes(path);
			if (attributes.HasFlag(FileAttributes.Directory))
				return null;

			try
			{
				ZipModPackage package = new ZipModPackage(path);
				if (package.Exists(MetadataFile))
				{
					string metadataText = async ? await package.ReadTextAsync(MetadataFile) : package.ReadText(MetadataFile);
					if (metadataText != null)
					{
						package.Metadata = new JSONResourceReader().Read<ModMetadata>(metadataText);
						return package;
					}
				}
			}
			catch (Exception)
			{
			}

			return null;
		}
	}

	public class ZipModPackage : ModPackage
	{
		public FileStream Stream { get; protected set; }
		public ZipArchive Archive { get; protected set; }

		public ZipModPackage(string path)
		{
			Path = path;
			Stream = new FileStream(path, FileMode.Open);
			Archive = new ZipArchive(Stream, ZipArchiveMode.Read);
		}

		public override bool Exists(string path)
		{
			return Archive.GetEntry(path) != null;
		}

		protected override async Task<string> ReadTextInternal(string path, bool async)
		{
			string matching = FindFile(path);
			if (matching == null)
				throw new FileNotFoundException(GetNotFoundException(path));

			ZipArchiveEntry entry = Archive.GetEntry(matching);
			Stream stream = entry.Open();
			TextReader text = new StreamReader(stream);
			string data = null;
			if (async)
				data = await text.ReadToEndAsync();
			else
				data = text.ReadToEnd();
			text.Dispose();
			stream.Dispose();
			return data;
		}

		protected override async Task<byte[]> ReadBytesInternal(string path, bool async)
		{
			string matching = FindFile(path);
			if (matching == null)
				throw new FileNotFoundException(GetNotFoundException(path));

			ZipArchiveEntry entry = Archive.GetEntry(matching);
			Stream stream = entry.Open();
			byte[] data = new byte[entry.Length];
			if (async)
				await stream.ReadAsync(data, 0, (int)entry.Length);
			else
				stream.Read(data, 0, (int)entry.Length);
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
									.Where(	e => string.Compare(fileDir, System.IO.Path.GetDirectoryName(e.FullName), true) == 0 &&
											e.Name != null &&
											string.Compare(fileName, System.IO.Path.GetFileNameWithoutExtension(e.Name), true) == 0)
									.Select(e => e.FullName);

				if (matching.Any())
					return matching.ToList<string>();
			}

			return null;
		}

		public override void Unload()
		{
			Archive.Dispose();
			Stream.Dispose();
		}
	}
}