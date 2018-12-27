using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using UnityEngine;

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
						package.Metadata = ToJSON<ModMetadata>(metadataText);
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
			ZipArchiveEntry entry = FindFile(path);
			if (entry == null)
				throw new FileNotFoundException(GetNotFoundException(path));

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
			ZipArchiveEntry entry = FindFile(path);
			if (entry == null)
				throw new FileNotFoundException(GetNotFoundException(path));
	
			Stream stream = entry.Open();
			byte[] data = new byte[entry.Length];
			if (async)
				await stream.ReadAsync(data, 0, (int)entry.Length);
			else
				stream.Read(data, 0, (int)entry.Length);
			stream.Dispose();
			return data;
		}

		public virtual ZipArchiveEntry FindFile(string path)
		{
			ZipArchiveEntry result = Archive.GetEntry(path);
			if (result != null)
				return result;

			if (! System.IO.Path.HasExtension(path))
			{
				string fileName = System.IO.Path.GetFileName(path);
				string fileDir = System.IO.Path.GetDirectoryName(path);
				foreach (ZipArchiveEntry entry in Archive.Entries)
				{
					string entryDir = System.IO.Path.GetDirectoryName(entry.FullName);
					if (string.Compare(fileDir, entryDir, true) == 0)
					{
						string entryName = entry.Name;
						if (entry.Name != "")
						{
							string withoutExt = System.IO.Path.GetFileNameWithoutExtension(entryName);
							if (string.Compare(fileName, withoutExt, true) == 0)
								return entry;
						}
					}
				}
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