using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using Modding.Resource.Readers;

namespace Modding.Loaders
{
	public class DirectModLoader: ModLoader
	{
        public string MetadataFile;

        public DirectModLoader(string metadataFile = "Metadata.json")
        {
            MetadataFile = metadataFile;
        }

		protected override async Task<ModPackage> LoadModInternal(string path, bool async)
		{
			FileAttributes attributes = File.GetAttributes(path);
			if (!attributes.HasFlag(FileAttributes.Directory))
				return null;

			try
			{
				DirectModPackage package = new DirectModPackage(path);
				string metadataPath = package.GetFullPath(MetadataFile);
				if (package.Exists(metadataPath))
				{
					string metadataText = async ? await package.ReadTextAsync(metadataPath) : package.ReadText(metadataPath);
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

	public class DirectModPackage: ModPackage
	{
		public DirectModPackage(string path)
		{
			Path = path;
		}

		protected override async Task<string> ReadTextInternal(string path, bool async)
		{
			string matching = FindFile(path);
			if (matching == null)
				throw GetNotFoundException(path);

			matching = GetFullPath(matching);
			if (async)
				using (StreamReader stream = new StreamReader(matching))
					return await stream.ReadToEndAsync();
			else
				return File.ReadAllText(matching);
		}

		protected override async Task<byte[]> ReadBytesInternal(string path, bool async)
		{
			string matching = FindFile(path);
			if (matching == null)
				throw GetNotFoundException(path);

			matching = GetFullPath(matching);
			if (async)
			{
				byte[] content;
				using (FileStream stream = new FileStream(matching, FileMode.Open))
				{
					content = new byte[stream.Length];
					await stream.ReadAsync(content, 0, (int)stream.Length);
				}
				return content;
			}
			else
				return File.ReadAllBytes(matching);
		}

		public override IEnumerable<string> FindFiles(string path)
		{
			string fullPath = GetFullPath(path);
			if (File.Exists(fullPath))
				return new string[] { path };
			
			if (!System.IO.Path.HasExtension(path))
			{
				string relativeDir = System.IO.Path.GetDirectoryName(path);
				string fullDir = System.IO.Path.GetDirectoryName(fullPath);
				string fullFile = System.IO.Path.GetFileName(fullPath);
				var matching = Directory.EnumerateFiles(fullDir, $"{fullFile}.*")
					.Select(p => System.IO.Path.Combine(relativeDir, System.IO.Path.GetFileName(p)));
				if (matching.Any())
					return matching;
			}

			return null;
		}

		public override bool Exists(string path)
		{
			return File.Exists(GetFullPath(path));
		}

		public virtual string GetFullPath(string subPath)
		{
			return System.IO.Path.Combine(Path, subPath);
		}

		public override void Unload()
		{
		}
	}
}