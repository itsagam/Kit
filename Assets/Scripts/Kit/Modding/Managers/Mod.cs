#if MODDING
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Kit.Modding.Scripting;
using Kit.Parsers;
using UnityEngine;
using XLua;

namespace Kit.Modding
{
	/// <summary>Information about the mod stored in <see cref="ModLoader.MetadataFile" />.</summary>
	public class ModMetadata
	{
		/// <summary>Name of the mod.</summary>
		public string Name;

		/// <summary>Mod version.</summary>
		public string Version;

		/// <summary>Mod author.</summary>
		public string Author;

		/// <summary>A short description of the mod.</summary>
		public string Description;

		/// <summary>A mode for executing scripts.</summary>
		public ModPersistence Persistence;

		/// <summary>Relative paths to scripts to execute.</summary>
		public List<string> Scripts;
	}

	/// <summary>Defines a mode for executing mod scripts.</summary>
	public enum ModPersistence
	{
		/// <summary>Just execute.</summary>
		None,

		/// <summary>Execute and create a <see cref="SimpleDispatcher"/>.</summary>
		Simple,

		/// <summary>Execute and create a <see cref="FullDispatcher"/>.</summary>
		Full
	}

	/// <summary>Base class for mods loaded by a <see cref="ModLoader"/>.</summary>
	public abstract class Mod
	{
		#region Fields

		/// <summary>Garbage collector interval for the Lua environment of the mod.</summary>
		public const float GCInterval = 1.0f;

		private static YieldInstruction gcYield = new WaitForSeconds(GCInterval);

		/// <summary>The <see cref="ModGroup"/> this mod belong to.</summary>
		public ModGroup Group { get; set; }

		/// <summary>The mod's metadata.</summary>
		public ModMetadata Metadata { get; set; }

		/// <summary>Find files based on a path.</summary>
		/// <param name="path">The path.</param>
		/// <returns>List of matching files.</returns>
		public abstract IEnumerable<string> FindFiles(string path);

		/// <summary>Returns whether a file exists.</summary>
		/// <param name="path">Path to the file.</param>
		public abstract bool Exists(string path);

		/// <summary>Read a file from the mod in text-mode.</summary>
		/// <param name="path">Path to the file.</param>
		/// <returns>Contents of the file as a <see cref="string"/>.</returns>
		public abstract string ReadText(string path);

		/// <inheritdoc cref="ReadText(string)"/>
		/// <summary>Read a file from the mod asynchronously in text-mode.</summary>
		public abstract UniTask<string> ReadTextAsync(string path);

		/// <summary>Read a file in binary-mode from the mod.</summary>
		/// <param name="path">Path to the file.</param>
		/// <returns>Contents of the file as a byte array.</returns>
		public abstract byte[] ReadBytes(string path);

		/// <inheritdoc cref="ReadBytes(string)"/>
		/// <summary>Read a file from the mod asynchronously in binary-mode.</summary>
		public abstract UniTask<byte[]> ReadBytesAsync(string path);

		/// <summary>The scripting environment associated with this mod.</summary>
		public LuaEnv ScriptEnv { get; protected set; }

		/// <summary>The <see cref="ScriptDispatcher"/> associated with this mod.</summary>
		public SimpleDispatcher ScriptDispatcher { get; protected set; }

		#endregion

		#region Resources

		/// <summary>Load a resource.</summary>
		/// <param name="folder">The folder to load the resource from.</param>
		/// <param name="file">The path and file-name relative to the <paramref name="folder" />.</param>
		/// <returns>Reference to the resource.</returns>
		public object Load(ResourceFolder folder, string file)
		{
			return LoadEx(typeof(object), ModManager.GetModdingPath(folder, file)).reference;
		}

		/// <inheritdoc cref="Load(ResourceFolder, string)"/>
		/// <typeparam name="T">Type of the resource expected.</typeparam>
		public T Load<T>(ResourceFolder folder, string file)
		{
			return (T) LoadEx(typeof(T), ModManager.GetModdingPath(folder, file)).reference;
		}

		/// <inheritdoc cref="Load(ResourceFolder, string)"/>
		/// <param name="type">Type of the resource expected.</param>
		public object Load(Type type, ResourceFolder folder, string file)
		{
			return LoadEx(type, ModManager.GetModdingPath(folder, file)).reference;
		}

		/// <inheritdoc cref="Load(ResourceFolder, string)"/>
		/// <param name="path">Path to the resource.</param>
		public object Load(string path)
		{
			return LoadEx(typeof(object), path).reference;
		}

		/// <inheritdoc cref="Load(string)"/>
		/// <typeparam name="T">Type of the resource expected.</typeparam>
		public T Load<T>(string path)
		{
			return (T) LoadEx(typeof(T), path).reference;
		}

		/// <inheritdoc cref="Load(string)"/>
		/// <param name="type">Type of the resource expected.</param>
		public object Load(Type type, string path)
		{
			return LoadEx(type, path).reference;
		}

		/// <inheritdoc cref="Load(string)"/>
		/// <returns>Reference to the resource, matched file's path, and the parser used to decode it.</returns>
		public virtual (object reference, string filePath, ResourceParser parser) LoadEx(Type type, string path)
		{
			var matchingFiles = FindFiles(path);
			var certainties = RankParsers(type, matchingFiles);
			string text = null;
			byte[] bytes = null;
			foreach ((string filePath, ResourceParser parser, var _) in certainties)
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

					if (text == null)
					{
						text = ReadText(filePath);
						if (text == null)
							return default;
					}

					return (parser.Read(type, text, filePath), filePath, parser);
				}
				catch
				{
				}

			return default;
		}

		/// <inheritdoc cref="Load(ResourceFolder, string)"/>
		/// <summary>Load a resource asynchronously.</summary>
		public async UniTask<object> LoadAsync(ResourceFolder folder, string file)
		{
			return (await LoadExAsync(typeof(object), ModManager.GetModdingPath(folder, file))).reference;
		}

		/// <inheritdoc cref="Load{T}(ResourceFolder, string)"/>
		/// <summary>Load a resource asynchronously.</summary>
		public async UniTask<T> LoadAsync<T>(ResourceFolder folder, string file)
		{
			return (T) (await LoadExAsync(typeof(T), ModManager.GetModdingPath(folder, file))).reference;
		}

		/// <inheritdoc cref="Load(Type, ResourceFolder, string)"/>
		/// <summary>Load a resource asynchronously.</summary>
		public async UniTask<object> LoadAsync(Type type, ResourceFolder folder, string file)
		{
			return (await LoadExAsync(type, ModManager.GetModdingPath(folder, file))).reference;
		}

		/// <inheritdoc cref="Load(string)"/>
		/// <summary>Load a resource asynchronously.</summary>
		public async UniTask<object> LoadAsync(string path)
		{
			return (await LoadExAsync(typeof(object), path)).reference;
		}

		/// <inheritdoc cref="Load{T}(string)"/>
		/// <summary>Load a resource asynchronously.</summary>
		public async UniTask<T> LoadAsync<T>(string path)
		{
			return (T) (await LoadExAsync(typeof(T), path)).reference;
		}

		/// <inheritdoc cref="Load(Type, string)"/>
		/// <summary>Load a resource asynchronously.</summary>
		public async UniTask<object> LoadAsync(Type type, string path)
		{
			return (await LoadExAsync(type, path)).reference;
		}

		/// <inheritdoc cref="LoadEx(Type, string)"/>
		/// <summary>Load a resource asynchronously.</summary>
		public virtual async UniTask<(object reference, string filePath, ResourceParser parser)> LoadExAsync(Type type, string path)
		{
			var matchingFiles = FindFiles(path);
			var certainties = RankParsers(type, matchingFiles);
			string text = null;
			byte[] bytes = null;
			foreach ((string filePath, ResourceParser parser, var _) in certainties)
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

					if (text == null)
					{
						text = await ReadTextAsync(filePath);
						if (text == null)
							return default;
					}

					return (parser.Read(type, text, filePath), filePath, parser);
				}
				catch
				{
				}

			return default;
		}

		/// <summary>Ranks all registered parsers according to their certainty of parsing files.</summary>
		/// <param name="type">Type of the object expected.</param>
		/// <param name="files">List of files.</param>
		/// <returns>A list of files and parsers ordered by certainty.</returns>
		protected static IEnumerable<(string filePath, ResourceParser parser, float certainty)> RankParsers(Type type,
																											IEnumerable<string> files)
		{
			return files
				  .SelectMany(filePath => ResourceManager.Parsers.Select(parser => (filePath, parser,
																					certainty: parser.CanParse(type, filePath))))
				  .Where(tuple => tuple.certainty > 0)
				  .OrderByDescending(tuple => tuple.certainty);
		}

		#endregion

		#region Scripting

		/// <summary>Setups the scripting environment.</summary>
		protected virtual IEnumerable<string> SetupScripting()
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

		/// <summary>Execute scripts in the mod.</summary>
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

		/// <summary>Execute scripts in the mod asynchronously.</summary>
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

		protected virtual void CreateDispatcher()
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

		protected virtual void HookOrDispose()
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

		protected virtual IEnumerator TickCoroutine()
		{
			while (true)
			{
				yield return gcYield;
				ScriptEnv.Tick();
			}
		}

		protected virtual void ExecuteSafe(Action action)
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

		protected virtual void DisposeScripting()
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

		/// <summary>Unloads the mod.</summary>
		public virtual void Unload()
		{
			DisposeScripting();
		}

		#endregion
	}
}
#endif