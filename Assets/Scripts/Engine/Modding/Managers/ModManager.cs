using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Modding.Parsers;

#if MODDING
using UniRx.Async;
using Modding.Loaders;
#endif

public enum ResourceFolder
{
	Data,
	StreamingAssets,
	PersistentData,
	Resources
}

namespace Modding
{
#if MODDING
	public struct ResourceInfo
	{
		public Mod Mod;
		public string Path;
		public ResourceParser Parser;
		public WeakReference Reference;
		
		public ResourceInfo(Mod mod, string file, ResourceParser parser, object reference) 
			: this(mod, file, parser, new WeakReference(reference)) 
		{
		}

		public ResourceInfo(Mod mod, string file, ResourceParser parser, WeakReference reference)
		{
			Mod = mod;
			Path = file;
			Parser = parser;
			Reference = reference;
		}
	}
	#endif

	public class ModManager
	{
		public static List<ResourceParser> Parsers = new List<ResourceParser>() {
			new JSONParser(),
			new TextureParser(),
			new AudioParser(),
			new TextParser()
		};

#if MODDING
		public static event Action<Mod> ModLoaded;
		public static event Action<Mod> ModUnloaded;
		public static event Action<ResourceFolder, string, ResourceInfo> ResourceLoaded;
		public static event Action<ResourceFolder, string, ResourceInfo> ResourceReused;
		public static event Action<ResourceFolder, string, Mod> ResourceUnloaded;

		public const string DefaultModFolderName = "Mods";

		public static List<string> SearchPaths = new List<string>();
		public static List<ModLoader> Loaders = new List<ModLoader>() {
			//UNDONE: new AssetBundleLoader(),
			new DirectModLoader(),
			new ZipModLoader()
		};
		
		protected static List<Mod> mods = new List<Mod>();
		protected static Dictionary<(Type type, ResourceFolder folder, string file), ResourceInfo> cachedResources 
			= new Dictionary<(Type, ResourceFolder, string), ResourceInfo>();
		protected static Dictionary<ResourceFolder, string> folderToString = new Dictionary<ResourceFolder, string>();

		static ModManager()
		{
			AddDefaultSearchPaths();
			CacheFolderNames();
		}

		protected static void AddDefaultSearchPaths()
        {
			// UNDONE: Adding "Patches" and patching system
			string writeableFolder = GetWriteableFolder();
			SearchPaths.Add(writeableFolder + DefaultModFolderName + "/");
		}

		protected static string GetWriteableFolder()
		{
			string folder = null;
			switch (Application.platform)
			{
				case RuntimePlatform.WindowsEditor:
				case RuntimePlatform.OSXEditor:
				case RuntimePlatform.LinuxEditor:
				case RuntimePlatform.WindowsPlayer:
				case RuntimePlatform.LinuxPlayer:
					folder = Path.GetDirectoryName(Application.dataPath);
					break;

				case RuntimePlatform.OSXPlayer:
				case RuntimePlatform.IPhonePlayer:
				case RuntimePlatform.Android:
					folder = Application.persistentDataPath;
					break;
			}
			if (folder != null)
				folder += "/";
			return folder;
		}

		// The "+" operator and Path.Combine are really costly and have a huge perfomance impact
		protected static void CacheFolderNames()
		{
			foreach (ResourceFolder value in Enum.GetValues(typeof(ResourceFolder)))
				folderToString[value] = Enum.GetName(typeof(ResourceFolder), value) + "/";
		}

		public static void LoadMods(bool executeScripts = false)
		{
			foreach (string path in SearchPaths)
			{
				if (!Directory.Exists(path))
					continue;

				string[] childPaths = Directory.GetFileSystemEntries(path);
				foreach (string childPath in childPaths)
				{
					foreach (ModLoader loader in Loaders)
					{
						Mod mod = loader.LoadMod(childPath);
						if (mod != null)
						{
							mods.Add(mod);
							break;
						}
					}
				}
			}

			LoadModOrder();
			SaveModOrder();

			if (executeScripts)
				ExecuteScripts();
		}

		public static async UniTask LoadModsAsync(bool executeScripts = false)
		{
			foreach (string path in SearchPaths)
			{
				if (!Directory.Exists(path))
					continue;

				string[] childPaths = Directory.GetFileSystemEntries(path);
				foreach (string childPath in childPaths)
				{
					foreach (ModLoader loader in Loaders)
					{
						Mod mod = await loader.LoadModAsync(childPath);
						if (mod != null)
						{
							mods.Add(mod);
							break;
						}
					}
				}
			}

			LoadModOrder();
			SaveModOrder();

			if (executeScripts)
				await ExecuteScriptsAsync();
		}

		public static void ExecuteScripts()
		{
			foreach (Mod mod in mods)
			{
				mod.ExecuteScripts();
				ModLoaded?.Invoke(mod);
			}
		}

		public static async UniTask ExecuteScriptsAsync()
		{
			foreach (Mod mod in mods)
			{
				await mod.ExecuteScriptsAsync();
				ModLoaded?.Invoke(mod);
			}
		}

		// UNDONE: Make a "Mods" UI, allowing to change mod order and displaying mod information
		public static void LoadModOrder()
		{
			// Reversing the list makes sure new mods (whose entries we do not have and will all return -1) are ordered in reverse
			mods = mods.AsEnumerable().Reverse().OrderBy(m => PlayerPrefs.GetInt($"Mods/{m.Metadata.Name}.Order", -1)).ToList();
		}
	
		public static int GetModOrder(Mod mod)
		{
			return mods.FindIndex(p => p == mod);
		}

		public static void MoveModFirst(Mod mod)
		{
			MoveModOrder(mod, 0);
		}

		public static void MoveModLast(Mod mod)
		{
			MoveModOrder(mod, mods.Count-1);
		}

		public static void MoveModUp(Mod mod)
		{
			MoveModOrder(mod, GetModOrder(mod) - 1);
		}

		public static void MoveModDown(Mod mod)
		{
			MoveModOrder(mod, GetModOrder(mod) + 1);
		}

		public static void MoveModOrder(Mod mod, int index)
		{
			if (index < 0 || index >= mods.Count)
				return;

			mods.Remove(mod);
			mods.Insert(index, mod);
			SaveModOrder();
		}

		public static void SaveModOrder()
		{
			for (int i = 0; i < mods.Count; i++)
				PlayerPrefs.SetInt($"Mods/{mods[i].Metadata.Name}.Order", i);
		}

		protected static T LoadCached<T>(ResourceFolder folder, string file) where T : class
		{
			if (cachedResources.TryGetValue((typeof(T), folder, file), out ResourceInfo resource))
			{
				object reference = resource.Reference.Target;
				if (reference != null)
				{
					ResourceReused?.Invoke(folder, file, resource);
					return (T) reference;
				}
			}
			return null;
		}

		public static T Load<T>(ResourceFolder folder, string file) where T : class
		{
			T cachedReference = LoadCached<T>(folder, file);
			if (cachedReference != null)
				return cachedReference;

			string path = GetModdingPath(folder, file);
			foreach (Mod mod in mods)
			{
				var (reference, filePath, parser) = mod.Load<T>(path);
				if (reference != null)
				{
					ResourceInfo resource = new ResourceInfo(mod, filePath, parser, reference);
					cachedResources[(typeof(T), folder, file)] = resource;
					ResourceLoaded?.Invoke(folder, file, resource);
					return reference;
				}
			}

			return null;
		}

		public static async UniTask<T> LoadAsync<T>(ResourceFolder folder, string file) where T : class
		{
			T cachedReference = LoadCached<T>(folder, file);
			if (cachedReference != null)
				return cachedReference;

			string path = GetModdingPath(folder, file);
			foreach (Mod mod in mods)
			{
				var (reference, filePath, parser) = await mod.LoadAsync<T>(path);
				if (reference != null)
				{
					ResourceInfo resource = new ResourceInfo(mod, filePath, parser, reference);
					cachedResources[(typeof(T), folder, file)] = resource;
					ResourceLoaded?.Invoke(folder, file, resource);
					return reference;
				}
			}

			return null;
		}

		public static List<T> LoadAll<T>(ResourceFolder folder, string file) where T : class
		{
			return LoadAll<T>(GetModdingPath(folder, file));
		}

		public static List<T> LoadAll<T>(string path) where T : class
		{
			List<T> all = new List<T>();
			foreach (Mod mod in mods)
			{
				var (reference, filePath, parser) = mod.Load<T>(path);
				all.Add(reference);
			}
			return all;
		}

		public static UniTask<List<T>> LoadAllAsync<T>(ResourceFolder folder, string file) where T : class
		{
			return LoadAllAsync<T>(GetModdingPath(folder, file));
		}

		public static async UniTask<List<T>> LoadAllAsync<T>(string path) where T : class
		{
			List<T> all = new List<T>();
			foreach (Mod mod in mods)
			{
				var (reference, filePath, parser) = await mod.LoadAsync<T>(path);
				all.Add(reference);
			}
			return all;
		}

		public static ResourceInfo GetResourceInfo<T>(ResourceFolder folder, string file)
		{
			if (cachedResources.TryGetValue((typeof(T), folder, file), out ResourceInfo resource))
				return resource;
			return default;
		}

		public static ResourceInfo GetResourceInfo(object resource)
		{
			return cachedResources.FirstOrDefault(r => r.Value.Reference.Target == resource).Value;
		}

		public static string ReadText(ResourceFolder folder, string file)
		{
			return ReadText(GetModdingPath(folder, file));
		}

		public static string ReadText(string path)
		{
			foreach (Mod mod in mods)
			{
				try
				{
					return mod.ReadText(path);
				}
				catch
				{
				}
			}
			return null;
		}

		public static UniTask<string> ReadTextAsync(ResourceFolder folder, string file)
		{
			return ReadTextAsync(GetModdingPath(folder, file));
		}

		public static async UniTask<string> ReadTextAsync(string path)
		{
			foreach (Mod mod in mods)
			{
				try
				{
					return await mod.ReadTextAsync(path);
				}
				catch
				{
				}
			}
			return null;
		}

		public static List<string> ReadTextAll(ResourceFolder folder, string file)
		{
			return ReadTextAll(GetModdingPath(folder, file));
		}

		public static List<string> ReadTextAll(string path)
		{
			List<string> all = new List<string>();
			foreach (Mod mod in mods)
			{
				try
				{
					all.Add(mod.ReadText(path));
				}
				catch
				{
				}
			}
			return all;
		}

		public static UniTask<List<string>> ReadTextAllAsync(ResourceFolder folder, string file)
		{
			return ReadTextAllAsync(GetModdingPath(folder, file));
		}

		public static async UniTask<List<string>> ReadTextAllAsync(string path)
		{
			List<string> all = new List<string>();
			foreach (Mod mod in mods)
			{
				try
				{
					all.Add(await mod.ReadTextAsync(path));
				}
				catch
				{
				}
			}
			return all;
		}

		public static byte[] ReadBytes(ResourceFolder folder, string file)
		{
			return ReadBytes(GetModdingPath(folder, file));
		}

		public static byte[] ReadBytes(string path)
		{
			foreach (Mod mod in mods)
			{
				try
				{
					return mod.ReadBytes(path);
				}
				catch
				{
				}
			}
			return null;
		}

		public static UniTask<byte[]> ReadBytesAsync(ResourceFolder folder, string file)
		{
			return ReadBytesAsync(GetModdingPath(folder, file));
		}

		public static async UniTask<byte[]> ReadBytesAsync(string path)
		{
			foreach (Mod mod in mods)
			{
				try
				{
					return await mod.ReadBytesAsync(path);
				}
				catch
				{
				}
			}
			return null;
		}

		public static List<byte[]> ReadBytesAll(ResourceFolder folder, string file)
		{
			return ReadBytesAll(GetModdingPath(folder, file));
		}

		public static List<byte[]> ReadBytesAll(string path)
		{
			List<byte[]> all = new List<byte[]>();
			foreach (Mod mod in mods)
			{
				try
				{
					all.Add(mod.ReadBytes(path));
				}
				catch
				{
				}
			}
			return all;
		}

		public static UniTask<List<byte[]>> ReadBytesAllAsync(ResourceFolder folder, string file)
		{
			return ReadBytesAllAsync(GetModdingPath(folder, file));
		}

		public static async UniTask<List<byte[]>> ReadBytesAllAsync(string path)
		{
			List<byte[]> all = new List<byte[]>();
			foreach (Mod mod in mods)
			{
				try
				{
					all.Add(await mod.ReadBytesAsync(path));
				}
				catch
				{
				}
			}
			return all;
		}

		public static void UnloadMods(bool destroyLoaded = false)
		{
			for (int i = mods.Count - 1; i >= 0; i--)
				UnloadMod(mods[i], destroyLoaded);
		}

		public static void UnloadMod(Mod mod, bool destroyLoaded = true)
		{
			if (destroyLoaded)
				UnloadAll(mod);
			mod.Unload();
			mods.Remove(mod);
			ModUnloaded?.Invoke(mod);
		}

		public static bool UnloadAll(Mod mod)
		{
			bool found = false;
			foreach (var kvp in cachedResources.Reverse())
			{
				ResourceInfo resource = kvp.Value;
				var key = kvp.Key;
				if (resource.Mod == mod)
				{
					UnloadInternal(resource.Reference.Target);
					cachedResources.Remove(key);
					found = true;
					ResourceUnloaded?.Invoke(key.folder, key.file, resource.Mod);
				}
			}
			return found;
		}

		public static bool Unload(object reference)
		{
			var kvp = cachedResources.FirstOrDefault(k => k.Value.Reference.Target == reference);
			var key = kvp.Key;
			// Because of FirstOrDefault, kvp.file will be null if key is not found
			if (key.file != null)
			{
				ResourceInfo resource = kvp.Value;
				UnloadInternal(resource.Reference.Target);
				cachedResources.Remove(key);
				ResourceUnloaded?.Invoke(key.folder, key.file, resource.Mod);
				return true;
			}
			return false;
		}

		public static bool Unload<T>(ResourceFolder folder, string file)
		{
			var key = (typeof(T), folder, file);
			if (cachedResources.TryGetValue(key, out ResourceInfo resource))
			{
				UnloadInternal(resource.Reference.Target);
				cachedResources.Remove(key);
				ResourceUnloaded?.Invoke(folder, file, resource.Mod);
				return true;
			}
			return false;
		}

		protected static void UnloadInternal(object resource)
		{
			if (resource is UnityEngine.Object unityObject)
				UnityEngine.Object.Destroy(unityObject);
		}

		public static void ClearCache()
		{
			cachedResources.Clear();
		}

		public static string GetModdingPath(ResourceFolder folder)
		{
			return folderToString[folder];
		}

		public static string GetModdingPath(ResourceFolder folder, string file)
		{
			return GetModdingPath(folder) + file;
		}

		public static ReadOnlyCollection<Mod> Mods
		{
			get
			{
				return mods.AsReadOnly();
			}
		}
#endif
	}
}