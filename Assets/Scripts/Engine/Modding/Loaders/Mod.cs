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

namespace Modding
{
    public class ModMetadata
    {
        public string Name;
        public string Author;
		public string Description;
        public string Version;
		public bool Persistent;
		public List<string> Scripts;
    }

	public abstract class Mod
	{
		public const string MetadataFile = "Metadata.json";
		public const float GCInterval = 1.0f;
		public static Dictionary<string, Func<IObservable<long>>> UpdateDelegates = new Dictionary<string, Func<IObservable<long>>>
			{
				{ "update", Observable.EveryUpdate },
				{ "fixedUpdate", Observable.EveryFixedUpdate },
				{ "lateUpdate", Observable.EveryLateUpdate },
				{ "endOfFrame", Observable.EveryEndOfFrame }
			};

		public string Path { get;  protected set; }
		public ModMetadata Metadata { get; protected set; }

		public ModGroup Group { get; set; }

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

			var certainties = RankParsers(matchingFiles, typeof(T));
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
						return (parser.Read<T>(bytes, filePath), filePath, parser);
					}
					else
					{
						if (text == null)
							text = ReadText(filePath);
						return (parser.Read<T>(text, filePath), filePath, parser);
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

			var certainties = RankParsers(matchingFiles, typeof(T));
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
						return (parser.Read<T>(bytes, filePath), filePath, parser);
					}
					else
					{
						if (text == null)
							text = await ReadTextAsync(filePath);
						return (parser.Read<T>(text, filePath), filePath, parser);
					}
				}
				catch
				{
				}
			}

			return default;
		}

		protected static IEnumerable<(string filePath, ResourceParser parser, float certainty)> RankParsers(IEnumerable<string> matchingFiles, Type type)
		{
			return matchingFiles.SelectMany(filePath => ModManager.Parsers.Select(parser => (filePath, parser, certainty: parser.CanOperate(filePath, type))))
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
			
			return validScripts;
		}

		public virtual void ExecuteScripts()
		{
			var scripts = SetupScripting();
			if (scripts == null)
				return;

			foreach (string scriptFile in scripts)
			{
				var script = ReadBytes(scriptFile);
				ExecuteSafe(() => scriptEnv.DoString(script, scriptFile));
			}

			HookOrDispose();
		}

		public virtual async UniTask ExecuteScriptsAsync()
		{
			var scripts = SetupScripting();
			if (scripts == null)
				return;

			foreach (string scriptFile in scripts)
			{
				var script = await ReadBytesAsync(scriptFile);
				ExecuteSafe(() => scriptEnv.DoString(script, scriptFile));
			}

			HookOrDispose();
		}

		protected void HookOrDispose()
		{
			if (Metadata.Persistent)
				HookMethods();
			else
				DisposeScripting();
		}

		protected virtual void HookMethods()
		{
			Observable.Interval(TimeSpan.FromSeconds(GCInterval)).Subscribe(l => scriptEnv.Tick()).AddTo(scriptDisposables);

			Action awake = scriptEnv.Global.Get<Action>("awake");
			if (awake != null)
				ExecuteSafe(awake);

			Action start = scriptEnv.Global.Get<Action>("start");
			if (start != null)
				Observable.NextFrame().Subscribe(u => ExecuteSafe(start));

			foreach (var kvp in UpdateDelegates)
			{
				Action action = scriptEnv.Global.Get<Action>(kvp.Key);
				if (action != null)
					kvp.Value().Subscribe(l => ExecuteSafe(action)).AddTo(scriptDisposables);
			}
		}

		public void Schedule(string type, Action action)
		{
			if (UpdateDelegates.TryGetValue(type, out var function))
				function().Subscribe(l => ExecuteSafe(action)).AddTo(scriptDisposables);
		}

		public void StartCoroutine(IEnumerator enumerator)
		{
			MainThreadDispatcher.StartCoroutine(ExecuteSafe(enumerator));
		}

		protected IEnumerator ExecuteSafe(IEnumerator enumerator)
		{
			while (true)
			{
				object current;
				try
				{
					if (enumerator.MoveNext() == false)
						break;
					current = enumerator.Current;
				}
				catch (Exception e)
				{
					Debugger.Log("ModManager", $"{Metadata.Name} – {e.Message}");
					yield break;
				}
				yield return current;
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
			if (scriptEnv != null)
			{
				scriptEnv.Global.Get<Action>("destroy")?.Invoke();
				if (scriptDisposables != null)
				{
					scriptDisposables.Dispose();
					scriptDisposables = null;
				}				
				// HACK: Have to Dispose the environment the next frame as functions may hold references to prevent disposal
				Observable.NextFrame().Subscribe(u =>
				{
					scriptEnv.Dispose();
					scriptEnv = null;
				});
			}
		}

		public virtual void Unload()
		{
			DisposeScripting();
		}
	}
}
#endif