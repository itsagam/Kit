﻿using System;
using System.IO;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Modding.Parsers;

namespace Modding
{
    public class ModMetadata
    {
        public string Name;
        public string Author;
        public string Version;
        public object Other;
    }

	public abstract class ModPackage
	{
		public string Path { get;  protected set; }
		public ModMetadata Metadata { get; set ; }

		public abstract bool Exists(string path);
        public abstract void Unload();

		public abstract IEnumerable<string> FindFiles(string path);
		protected abstract Task<string> ReadTextInternal(string path, bool async);
		protected abstract Task<byte[]> ReadBytesInternal(string path, bool async);

		public virtual string FindFile(string path)
		{
			return FindFiles(path)?.FirstOrDefault();
		}

		public virtual T Load<T>(string path)
		{
			return LoadInternal<T>(path, false).Result;
		}

		public virtual async Task<T> LoadAsync<T>(string path)
		{
			return await LoadInternal<T>(path, true);
		}

		protected virtual async Task<T> LoadInternal<T>(string path, bool async)
		{
			// If File.json & File.jpg & Load<Texture>("Path/File"), it will load File.json

			IEnumerable<string> matchingFiles = FindFiles(path);
			if (matchingFiles == null)
				return default;

			foreach (string matchingFile in matchingFiles)
				foreach (ResourceParser parser in ModManager.Parsers)
					try
					{
						if (parser.CanRead<T>(matchingFile))
							if (parser.OperateWith == OperateType.Bytes)
								return (T)parser.Read<T>(await ReadBytesInternal(matchingFile, async), matchingFile);
							else
								return (T)parser.Read<T>(await ReadTextInternal(matchingFile, async), matchingFile);
					}
					catch (Exception)
					{
					}

			return default;
		}

		public virtual string ReadText(string path)
		{
			return ReadTextInternal(path, false).Result;
		}

		public virtual async Task<string> ReadTextAsync(string path)
		{
			return await ReadTextInternal(path, true);
		}

		public virtual byte[] ReadBytes(string path)
		{
			return ReadBytesInternal(path, false).Result;
		}

		public virtual async Task<byte[]> ReadBytesAsync(string path)
		{
			return await ReadBytesInternal(path, true);
		}

		protected virtual FileNotFoundException GetNotFoundException(string path)
        {
			return new FileNotFoundException($"File \"{path}\" not found in mod \"{Metadata.Name}\".");
        }
	}
}
 