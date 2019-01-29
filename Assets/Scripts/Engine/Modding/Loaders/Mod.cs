#if MODDING
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

// UNDONE: Find a way to set default values in MonoBehaviours (LoadMerged with JSON/Lua...)
// TODO: Provide hotfix functions
// TODO: Provide support for coroutines

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
		public const float GCInterval = 3.0f;
		public static Dictionary<string, Func<IObservable<long>>> UpdateDelegates = new Dictionary<string, Func<IObservable<long>>>
			{
				{ "update", Observable.EveryUpdate },
				{ "fixedUpdate", Observable.EveryFixedUpdate },
				{ "lateUpdate", Observable.EveryLateUpdate },
				{ "endOfFrame", Observable.EveryEndOfFrame }
			};

		public string Path { get;  protected set; }
		public ModMetadata Metadata { get; protected set; }
		
		public abstract IEnumerable<string> FindFiles(string path);
		public abstract bool Exists(string path);
		public abstract string ReadText(string path);
		public abstract UniTask<string> ReadTextAsync(string path);
		public abstract byte[] ReadBytes(string path);
		public abstract UniTask<byte[]> ReadBytesAsync(string path);

		protected LuaEnv scriptEnv;
		protected CompositeDisposable scriptDisposables;

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

		public virtual (T reference, string filePath, ResourceParser parser) Load<T>(string path) where T : class
		{
			IEnumerable<string> matchingFiles = FindFiles(path);
			if (matchingFiles == null)
				return default;

			var certainties = RankParsers<T>(matchingFiles);
			string text = null;
			byte[] bytes = null;
			foreach (var (filePath, parser, certainty) in certainties)
			{
				try
				{
					if (parser.OperateWith == OperateType.Bytes)
					{
						if (bytes == null)
							bytes = ReadBytes(filePath);
						return ((T) parser.Read<T>(bytes, filePath), filePath, parser);
					}
					else
					{
						if (text == null)
							text = ReadText(filePath);
						return ((T) parser.Read<T>(text, filePath), filePath, parser);
					}
				}
				catch
				{
				}
			}

			return default;
		}

		public virtual async UniTask<(T reference, string filePath, ResourceParser parser)> LoadAsync<T>(string path) where T : class
		{
			IEnumerable<string> matchingFiles = FindFiles(path);
			if (matchingFiles == null)
				return default;

			var certainties = RankParsers<T>(matchingFiles);
			string text = null;
			byte[] bytes = null;
			foreach (var (filePath, parser, certainty) in certainties)
			{
				try
				{
					if (parser.OperateWith == OperateType.Bytes)
					{
						if (bytes == null)
							bytes = await ReadBytesAsync(filePath);
						return ((T) parser.Read<T>(bytes, filePath), filePath, parser);
					}
					else
					{
						if (text == null)
							text = await ReadTextAsync(filePath);
						return ((T) parser.Read<T>(text, filePath), filePath, parser);
					}
				}
				catch
				{
				}
			}

			return default;
		}

		protected static IEnumerable<(string filePath, ResourceParser parser, float certainty)> RankParsers<T>(IEnumerable<string> matchingFiles) where T : class
		{
			return matchingFiles.SelectMany(filePath => ModManager.Parsers.Select(parser => (filePath, parser, certainty: parser.CanRead<T>(filePath))))
								.Where(d => d.certainty > 0)
								.OrderByDescending(d => d.certainty);
		}

		protected virtual IEnumerable<string> SetupScripting()
		{
			if (Metadata?.Scripts == null)
				return null;

			var validScripts = Metadata.Scripts.Where(s => !s.IsNullOrEmpty() && Exists(s));
			if (!validScripts.Any())
				return null;

			scriptEnv = new LuaEnv();
			scriptEnv.Global.Set("self", this);
			scriptEnv.DoString("require 'Lua/General'");
			scriptEnv.DoString("require 'Lua/Modding'");

			scriptDisposables = new CompositeDisposable();
			Observable.Interval(TimeSpan.FromSeconds(GCInterval)).Subscribe(l => scriptEnv.Tick()).AddTo(scriptDisposables);

			return validScripts;
		}

		public virtual void ExecuteScripts()
		{
			var scripts = SetupScripting();
			if (scripts == null)
				return;

			foreach (string scriptFile in scripts)
			{
				try
				{
					var script = ReadBytes(scriptFile);
					scriptEnv.DoString(script, scriptFile);
				}
				catch (Exception e)
				{
					Debugger.Log("ModManager", $"{Metadata.Name} – {e.Message}");
				}
			}
			HookMethods();
		}

		public virtual async UniTask ExecuteScriptsAsync()
		{
			var scripts = SetupScripting();
			if (scripts == null)
				return;

			foreach (string scriptFile in scripts)
			{
				try
				{
					var script = await ReadBytesAsync(scriptFile);
					scriptEnv.DoString(script, scriptFile);
				}
				catch (Exception e)
				{
					Debugger.Log("ModManager", $"{Metadata.Name} – {e.Message}");
				}
			}
			HookMethods();
		}

		protected virtual void HookMethods()
		{
			scriptEnv.Global.Get<Action>("awake")?.Invoke();

			Action start = scriptEnv.Global.Get<Action>("start");
			if (start != null)
				Observable.NextFrame().Subscribe(u => start());

			foreach (var kvp in UpdateDelegates)
			{
				Action action = scriptEnv.Global.Get<Action>(kvp.Key);
				if (action != null)
					kvp.Value().Subscribe(l => action()).AddTo(scriptDisposables);
			}
		}

		public void Schedule(string type, Action action)
		{
			if (UpdateDelegates.TryGetValue(type, out var function))
				function().Subscribe(l => action()).AddTo(scriptDisposables);
		}

		public virtual void Unload()
		{
			if (scriptEnv != null)
			{
				scriptEnv.Global.Get<Action>("destroy")?.Invoke();
				scriptDisposables.Dispose();
				// Have to Dispose the environment the next frame as functions may hold references to prevent disposal
				// TODO: Check whether NextFrame calls are Disposed automatically
				Observable.NextFrame().Subscribe(u => scriptEnv?.Dispose() );
			}
		}
	}
}
#endif