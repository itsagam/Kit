using System;
using System.IO;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Modding.Parsers;
using UniRx;
using XLua;

namespace Modding
{
    public class ModMetadata
    {
        public string Name;
        public string Author;
        public string Version;
		public List<string> Scripts;
    }

	public abstract class Mod
	{
		public string Path { get;  protected set; }
		public ModMetadata Metadata { get; set ; }
	
		public abstract IEnumerable<string> FindFiles(string path);
		public abstract bool Exists(string path);
		protected abstract Task<string> ReadTextInternal(string path, bool async);
		protected abstract Task<byte[]> ReadBytesInternal(string path, bool async);

		protected LuaEnv scriptEnv;
		protected IDisposable scriptUpdate;

		public void ExecuteScripts()
		{
			var task = ExecuteScriptsInternal(false);
		}

		public async Task ExecuteScriptsAsync()
		{
			await ExecuteScriptsInternal(true);
		}

		protected async Task ExecuteScriptsInternal(bool async)
		{
			if (Metadata?.Scripts == null)
				return;

			var validScripts = Metadata.Scripts.Where(s => !s.IsNullOrEmpty() && Exists(s));
			if (!validScripts.Any())
				return;

			// TODO: Share common scripting library & environment
			string luaLibrary = ResourceManager.ReadText(ResourceFolder.StreamingAssets, "Lua/LuaLibrary.lua", false);
			scriptEnv = new LuaEnv();
			scriptEnv.DoString(luaLibrary);

			foreach (string scriptFile in validScripts)
			{
				string script = async ? await ReadTextAsync(scriptFile) : ReadText(scriptFile);
				scriptEnv.DoString(script);
			}

			scriptUpdate = Observable.EveryUpdate().Subscribe(f => scriptEnv.Tick());
		}

		public virtual string FindFile(string path)
		{
			return FindFiles(path)?.FirstOrDefault();
		}

		public virtual (T reference, ResourceParser parser) Load<T>(string path) where T : class
		{
			return LoadInternal<T>(path, false).Result;
		}

		public virtual async Task<(T reference, ResourceParser parser)> LoadAsync<T>(string path) where T : class
		{
			return await LoadInternal<T>(path, true);
		}

		protected virtual async Task<(T reference, ResourceParser parser)> LoadInternal<T>(string path, bool async) where T : class
		{
			IEnumerable<string> matchingFiles = FindFiles(path);
			if (matchingFiles == null)
				return (null, null);
			
			var certainties = matchingFiles
								.SelectMany(file => ModManager.Parsers.Select(parser => (file, parser, certainty: parser.CanRead<T>(file))))
								.Where(d => d.certainty > 0)
								.OrderByDescending(d => d.certainty);
			string text = null;
			byte[] bytes = null;
			foreach (var (file, parser, certainty) in certainties)
			{
				try
				{
					if (parser.OperateWith == OperateType.Bytes)
					{
						if (bytes == null)
							bytes = await ReadBytesInternal(file, async);
						return ((T) parser.Read<T>(bytes, file), parser);
					}
					else
					{
						if (text == null)
							text = await ReadTextInternal(file, async);
						return ((T) parser.Read<T>(text, file), parser);
					}
				}
				catch (Exception)
				{
				}
			}

			return (null, null);
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

		public virtual void Unload()
		{
			scriptUpdate?.Dispose();
			scriptEnv?.Dispose();
		}
	}
}
 