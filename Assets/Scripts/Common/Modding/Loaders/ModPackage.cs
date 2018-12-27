using System;
using System.IO;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Modding.Resource;

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

		public virtual T Load<T>(string path) where T : UnityEngine.Object
		{
			return LoadInternal<T>(path, false).Result;
		}

		public virtual async Task<T> LoadAsync<T>(string path) where T : UnityEngine.Object
		{
			return await LoadInternal<T>(path, true);
		}

		protected virtual async Task<T> LoadInternal<T>(string path, bool async) where T : UnityEngine.Object
		{
			try
			{
				foreach(ResourceLoader loader in ModManager.ResourceLoaders)
					if (loader.CanLoad(typeof(T)))
					{
						if (loader.LoadWith == ReadType.Bytes)
							return loader.Load<T>(path, await ReadBytesInternal(path, async));
						else
							return loader.Load<T>(path, await ReadTextInternal(path, async));
					}
			}
			catch (Exception)
			{
			}

			return null;
		}

		public virtual T Read<T>(string path)
		{
			return ReadInternal<T>(path, false).Result;
		}

		public virtual async Task<T> ReadAsync<T>(string path)
		{
			return await ReadInternal<T>(path, true);
		}

		protected virtual async Task<T> ReadInternal<T>(string path, bool async)
		{
			try
			{
				IEnumerable<string> matchingFiles = FindFiles(path);
				foreach (string matchingFile in matchingFiles)
				{
					string fileName = System.IO.Path.GetFileName(matchingFile);
					foreach (ResourceReader reader in ModManager.ResourceReaders)
						if (reader.CanRead(fileName))
							if (reader.ReadWith == ReadType.Bytes)
								return reader.Read<T>(await ReadBytesInternal(matchingFile, async));
							else
								return reader.Read<T>(await ReadTextInternal(matchingFile, async));
				}
			}
			catch (Exception)
			{
			}
			
			return default(T);
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

		protected virtual string GetNotFoundException(string path)
        {
			return $"File \"{path}\" not found in mod \"{Metadata.Name}\".";
        }
	}
}