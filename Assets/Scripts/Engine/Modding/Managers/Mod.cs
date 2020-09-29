#if MODDING
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Engine.Modding.Scripting;
using Engine.Parsers;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XLua;

namespace Engine.Modding
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
		#region Fields
		public const float GCInterval = 1.0f;
		private static YieldInstruction gcYield = new WaitForSeconds(GCInterval);

		public ModGroup Group { get; set; }
		public ModMetadata Metadata { get; set; }

		public abstract IEnumerable<string> FindFiles(string path);
		public abstract bool Exists(string path);
		public abstract string ReadText(string path);
		public abstract UniTask<string> ReadTextAsync(string path);
		public abstract byte[] ReadBytes(string path);
		public abstract UniTask<byte[]> ReadBytesAsync(string path);

		public LuaEnv ScriptEnv { get; protected set; }
		public SimpleDispatcher ScriptDispatcher { get; protected set; }
		#endregion

		#region Resources
		public object Load(ResourceFolder folder, string file)
		{
			return LoadEx(typeof(object), ModManager.GetModdingPath(folder, file)).reference;
		}

		public T Load<T>(ResourceFolder folder, string file)
		{
			return (T) LoadEx(typeof(T), ModManager.GetModdingPath(folder, file)).reference;
		}

		public object Load(Type type, ResourceFolder folder, string file)
		{
			return LoadEx(type, ModManager.GetModdingPath(folder, file)).reference;
		}

		public object Load(string path)
		{
			return LoadEx(typeof(object), path).reference;
		}

		public T Load<T>(string path)
		{
			return (T) LoadEx(typeof(T), path).reference;
		}

		public object Load(Type type, string path)
		{
			return LoadEx(type, path).reference;
		}

		public virtual (object reference, string filePath, ResourceParser parser) LoadEx(Type type, string path)
		{
			var matchingFiles = FindFiles(path);
			var certainties = RankParsers(type, matchingFiles);
			string text = null;
			byte[] bytes = null;
			foreach (var (filePath, parser, _) in certainties)
				try
				{
					if (parser.ParseMode == ParseMode.Binary)
					{
						if (bytes == null)
						{
							bytes = ReadBytes(filePath);
							if (bytes == null)
								return default;
						}
						return (parser.Read(type, bytes, filePath), filePath, parser);
					}
					else
					{
						if (text == null)
						{
							text = ReadText(filePath);
							if (text == null)
								return default;
						}
						return (parser.Read(type, text, filePath), filePath, parser);
					}
				}
				catch
				{
				}

			return default;
		}

		public async UniTask<object> LoadAsync(ResourceFolder folder, string file)
		{
			return (await LoadExAsync(typeof(object), ModManager.GetModdingPath(folder, file))).reference;
		}

		public async UniTask<T> LoadAsync<T>(ResourceFolder folder, string file)
		{
			return (T) (await LoadExAsync(typeof(T), ModManager.GetModdingPath(folder, file))).reference;
		}

		public async UniTask<object> LoadAsync(Type type, ResourceFolder folder, string file)
		{
			return (await LoadExAsync(type, ModManager.GetModdingPath(folder, file))).reference;
		}

		public async UniTask<object> LoadAsync(string path)
		{
			return (await LoadExAsync(typeof(object), path)).reference;
		}

		public async UniTask<T> LoadAsync<T>(string path)
		{
			return (T) (await LoadExAsync(typeof(T), path)).reference;
		}

		public async UniTask<object> LoadAsync(Type type, string path)
		{
			return (await LoadExAsync(type, path)).reference;
		}

		public virtual async UniTask<(object reference, string filePath, ResourceParser parser)> LoadExAsync(Type type, string path)
		{
			var matchingFiles = FindFiles(path);
			var certainties = RankParsers(type, matchingFiles);
			string text = null;
			byte[] bytes = null;
			foreach (var (filePath, parser, _) in certainties)
				try
				{
					if (parser.ParseMode == ParseMode.Binary)
					{
						if (bytes == null)
						{
							bytes = await ReadBytesAsync(filePath);
							if (bytes == null)
								return default;
						}
						return (parser.Read(type, bytes, filePath), filePath, parser);
					}
					else
					{
						if (text == null)
						{
							text = await ReadTextAsync(filePath);
							if (text == null)
								return default;
						}
						return (parser.Read(type, text, filePath), filePath, parser);
					}
				}
				catch
				{
				}

			return default;
		}

		protected static IEnumerable<(string filePath, ResourceParser parser, float certainty)> RankParsers(Type type, IEnumerable<string> files)
		{
			return files
			      .SelectMany(filePath => ResourceManager.Parsers.Select(parser => (filePath, parser, certainty: parser.CanParse(type, filePath))))
			      .Where(tuple => tuple.certainty > 0)
			      .OrderByDescending(tuple => tuple.certainty);
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

		public void ExecuteScripts()
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

		public async UniTask ExecuteScriptsAsync()
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