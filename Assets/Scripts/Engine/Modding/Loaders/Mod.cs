#if MODDING
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Modding.Parsers;
using Modding.Scripting;
using UniRx.Async;
using XLua;

namespace Modding
{
    public class ModMetadata
    {
        public string Name;
        public string Author;
		public string Description;
        public string Version;
		public ModPersistence Persistence;
		public List<string> Scripts;
    }

	public enum ModPersistence
	{
		None,
		Simple,
		Full
	}

	public abstract class Mod
	{
		public const string MetadataFile = "Metadata.json";
		public const float GCInterval = 1.0f;
		private static YieldInstruction gcYield = new WaitForSeconds(GCInterval);

		public ModGroup Group { get; set; }
		public string Path { get;  protected set; }
		public ModMetadata Metadata { get; protected set; }

		public abstract IEnumerable<string> FindFiles(string path);
		public abstract bool Exists(string path);
		public abstract string ReadText(string path);
		public abstract UniTask<string> ReadTextAsync(string path);
		public abstract byte[] ReadBytes(string path);
		public abstract UniTask<byte[]> ReadBytesAsync(string path);

		public LuaEnv ScriptEnv { get; protected set; }
		public SimpleDispatcher ScriptDispatcher { get; protected set; }

		#region Initialization
		public virtual bool LoadMetadata()
		{
			string metadataText = ReadText(MetadataFile);
			if (metadataText != null)
			{
				Metadata = JSONParser.FromJson<ModMetadata>(metadataText);
				return true;
			}
			return false;
		}

		public virtual async UniTask<bool> LoadMetadataAsync()
		{
			string metadataText = await ReadTextAsync(MetadataFile);
			if (metadataText != null)
			{
				Metadata = JSONParser.FromJson<ModMetadata>(metadataText);
				return true;
			}
			return false;
		}
		#endregion

		#region Resources
		public object Load(ResourceFolder folder, string file)
		{
			return Load(typeof(object), folder, file);
		}

		public object Load(Type type, ResourceFolder folder, string file)
		{
			return Load(type, ModManager.GetModdingPath(folder, file));
		}

		public object Load(string path)
		{
			return Load(typeof(object), path);
		}

		public object Load(Type type, string path)
		{
			return LoadEx(type, path).reference;
		}

		public virtual (object reference, string filePath, ResourceParser parser) LoadEx(Type type, string path)
		{
			IEnumerable<string> matchingFiles = FindFiles(path);
			if (matchingFiles == null)
				return default;

			var certainties = RankParsers(type, matchingFiles);
			string text = null;
			byte[] bytes = null;
			foreach (var (filePath, parser, _) in certainties)
			{
				try
				{
					if (parser.OperateWith == OperateType.Bytes)
					{
						if (bytes == null)
							bytes = ReadBytes(filePath);
						return (parser.Read(type, bytes, filePath), filePath, parser);
					}
					else
					{
						if (text == null)
							text = ReadText(filePath);
						return (parser.Read(type, text, filePath), filePath, parser);
					}
				}
				catch
				{
				}
			}

			return default;
		}

		public UniTask<object> LoadAsync(ResourceFolder folder, string file)
		{
			return LoadAsync(typeof(object), folder, file);
		}

		public UniTask<object> LoadAsync(Type type, ResourceFolder folder, string file)
		{
			return LoadAsync(type, ModManager.GetModdingPath(folder, file));
		}

		public UniTask<object> LoadAsync(string path)
		{
			return LoadAsync(typeof(object), path);
		}

		public async UniTask<object> LoadAsync(Type type, string path)
		{
			return (await LoadExAsync(type, path)).reference;
		}

		public virtual async UniTask<(object reference, string filePath, ResourceParser parser)> LoadExAsync(Type type, string path)
		{
			IEnumerable<string> matchingFiles = FindFiles(path);
			if (matchingFiles == null)
				return default;

			var certainties = RankParsers(type, matchingFiles);
			string text = null;
			byte[] bytes = null;
			foreach (var (filePath, parser, _) in certainties)
			{
				try
				{
					if (parser.OperateWith == OperateType.Bytes)
					{
						if (bytes == null)
							bytes = await ReadBytesAsync(filePath);
						return (parser.Read(type, bytes, filePath), filePath, parser);
					}
					else
					{
						if (text == null)
							text = await ReadTextAsync(filePath);
						return (parser.Read(type, text, filePath), filePath, parser);
					}
				}
				catch
				{
				}
			}

			return default;
		}

		protected IEnumerable<(string filePath, ResourceParser parser, float certainty)> RankParsers(Type type, IEnumerable<string> matchingFiles)
		{
			return matchingFiles.SelectMany(filePath => ModManager.Parsers.Select(parser => (filePath, parser, certainty: parser.CanOperate(type, filePath))))
								.Where(d => d.certainty > 0)
								.OrderByDescending(d => d.certainty);
		}
		#endregion

		#region Scripting
		protected IEnumerable<string> SetupScripting()
		{
			if (Metadata.Scripts == null || Metadata.Scripts.Count == 0)
				return null;

			var validScripts = Metadata.Scripts.Where(s => !s.IsNullOrEmpty() && Exists(s));
			if (!validScripts.Any())
				return null;

			ScriptEnv = new LuaEnv();
			ScriptEnv.Global.Set("self", this);
			ScriptEnv.DoString("require 'Lua/General'");
			ScriptEnv.DoString("require 'Lua/Modding'");

			return validScripts;
		}

		public virtual void ExecuteScripts()
		{
			var scripts = SetupScripting();
			if (scripts == null)
				return;

			CreateDispatcher();

			foreach (string scriptFile in scripts)
			{
				var script = ReadBytes(scriptFile);
				ExecuteSafe(() => ScriptEnv.DoString(script, scriptFile));
			}

			HookOrDispose();
		}

		public virtual async UniTask ExecuteScriptsAsync()
		{
			var scripts = SetupScripting();
			if (scripts == null)
				return;

			CreateDispatcher();

			foreach (string scriptFile in scripts)
			{
				var script = await ReadBytesAsync(scriptFile);
				ExecuteSafe(() => ScriptEnv.DoString(script, scriptFile));
			}

			HookOrDispose();
		}

		protected void CreateDispatcher()
		{
			if (Metadata.Persistence != ModPersistence.None)
			{
				GameObject gameObject = new GameObject(Metadata.Name);
				ScriptDispatcher = Metadata.Persistence == ModPersistence.Simple ?
									   gameObject.AddComponent<SimpleDispatcher>() :
									   gameObject.AddComponent<FullDispatcher>();
				ScriptDispatcher.StartCoroutine(TickCoroutine());
			}
		}

		protected void HookOrDispose()
		{
			switch (Metadata.Persistence)
			{
				case ModPersistence.None:
					DisposeScripting();
					break;

				case ModPersistence.Simple:
					break;

				case ModPersistence.Full:
					((FullDispatcher) ScriptDispatcher).Hook(ScriptEnv);
					break;
			}
		}

		protected IEnumerator TickCoroutine()
		{
			while (true)
			{
				yield return gcYield;
				ScriptEnv.Tick();
			}
		}

		protected void ExecuteSafe(Action action)
		{
			try
			{
				action();
			}
			catch (Exception e)
			{
				Debugger.Log("ModManager", $"{Metadata.Name} – {e.Message}");
			}
		}

		protected void DisposeScripting()
		{
			if (ScriptEnv == null)
				return;

			if (ScriptDispatcher != null)
			{
				ScriptDispatcher.Stop();
				ScriptDispatcher.gameObject.Destroy();
				ScriptDispatcher = null;
			}

			ScriptEnv.Dispose();
			ScriptEnv = null;
		}
		#endregion

		#region Destruction
		public virtual void Unload()
		{
			DisposeScripting();
		}
		#endregion
	}
}
#endif