using System;
using System.IO;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

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
		public string Path { get; protected set; }
		public ModMetadata Metadata { get; protected set ; }

		public abstract bool Exists(string path);
        public abstract void Unload();

		protected abstract Task<string> ReadTextInternal(string path, bool async);
		protected abstract Task<byte[]> ReadBytesInternal(string path, bool async);

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
				foreach(ModParser parser in ModManager.Parsers)
					foreach(Type type in parser.SupportedTypes)
						if (typeof(T).IsAssignableFrom(type))
						{
							if (parser.ParseWith == ModParser.ParseType.Bytes)
								return parser.Parse<T>(path, await ReadBytesInternal(path, async));
							else
								return parser.Parse<T>(path, await ReadTextInternal(path, async));
						}
			}
			catch (Exception)
			{
			}

			return null;
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

		protected virtual T DecodeObject<T>(string encoded)
		{
			return JsonUtility.FromJson<T>(encoded);
		}

		protected virtual string EncodeObject(object data)
		{
			return JsonUtility.ToJson(data, true);
		}
	}
}