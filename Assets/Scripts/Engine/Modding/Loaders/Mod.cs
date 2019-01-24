using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Modding.Parsers;
using UniRx;
using UniRx.Async;
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
		public const string MetadataFile = "Metadata.json";

		public string Path { get;  protected set; }
		public ModMetadata Metadata { get; protected set; }
		
		public abstract IEnumerable<string> FindFiles(string path);
		public abstract bool Exists(string path);
		public abstract string ReadText(string path);
		public abstract UniTask<string> ReadTextAsync(string path);
		public abstract byte[] ReadBytes(string path);
		public abstract UniTask<byte[]> ReadBytesAsync(string path);

		protected LuaEnv scriptEnv;
		protected IDisposable scriptUpdate;

		public virtual bool LoadMetadata()
		{
			if (Exists(MetadataFile))
			{
				string metadataText = ReadText(MetadataFile);
				if (metadataText != null)
				{
					Metadata = JSONParser.FromJson<ModMetadata>(metadataText);
					return true;
				}
			}
			return false;
		}

		public virtual async UniTask<bool> LoadMetadataAsync()
		{
			if (Exists(MetadataFile))
			{
				string metadataText = await ReadTextAsync(MetadataFile);
				if (metadataText != null)
				{
					Metadata = JSONParser.FromJson<ModMetadata>(metadataText);
					return true;
				}
			}
			return false;
		}

		public virtual void ExecuteScripts()
		{
			if (Metadata?.Scripts == null)
				return;

			var validScripts = Metadata.Scripts.Where(s => !s.IsNullOrEmpty() && Exists(s));
			if (!validScripts.Any())
				return;

			scriptEnv = new LuaEnv();
			scriptEnv.Global.Set("self", this);
			scriptEnv.DoString("require 'Lua/General'");
			scriptEnv.DoString("require 'Lua/Modding'");
			foreach (string scriptFile in validScripts)
			{
				try
				{
					var script = ReadBytes(scriptFile);
					scriptEnv.DoString(script, scriptFile);
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}

			scriptUpdate = Observable.EveryUpdate().Subscribe(f => scriptEnv.Tick());
		}

		public virtual async UniTask ExecuteScriptsAsync()
		{
			if (Metadata?.Scripts == null)
				return;

			var validScripts = Metadata.Scripts.Where(s => !s.IsNullOrEmpty() && Exists(s));
			if (!validScripts.Any())
				return;

			scriptEnv = new LuaEnv();
			scriptEnv.Global.Set("self", this);
			scriptEnv.DoString("require 'Lua/General'");
			scriptEnv.DoString("require 'Lua/Modding'");
			foreach (string scriptFile in validScripts)
			{
				try
				{
					var script = await ReadBytesAsync(scriptFile);
					scriptEnv.DoString(script, scriptFile);
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}

			scriptUpdate = Observable.EveryUpdate().Subscribe(f => scriptEnv.Tick());
		}

		public virtual string FindFile(string path)
		{
			return FindFiles(path)?.FirstOrDefault();
		}

		public virtual (T reference, ResourceParser parser) Load<T>(string path) where T : class
		{
			IEnumerable<string> matchingFiles = FindFiles(path);
			if (matchingFiles == null)
				return default;

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
							bytes = ReadBytes(file);
						return ((T) parser.Read<T>(bytes, file), parser);
					}
					else
					{
						if (text == null)
							text = ReadText(file);
						return ((T) parser.Read<T>(text, file), parser);
					}
				}
				catch
				{
				}
			}

			return default;
		}

		public virtual async UniTask<(T reference, ResourceParser parser)> LoadAsync<T>(string path) where T : class
		{
			IEnumerable<string> matchingFiles = FindFiles(path);
			if (matchingFiles == null)
				return default;

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
							bytes = await ReadBytesAsync(file);
						return ((T) parser.Read<T>(bytes, file), parser);
					}
					else
					{
						if (text == null)
							text = await ReadTextAsync(file);
						return ((T) parser.Read<T>(text, file), parser);
					}
				}
				catch
				{
				}
			}

			return default;
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
 