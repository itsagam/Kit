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

namespace Modding
{
	public enum ResourceFolder
	{
		Data,
		StreamingAssets,
		PersistentData,
		Resources
	}

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
		protected static Dictionary<(Type type, ResourceFolder folder, string file), List<ResourceInfo>> cachedResources 
			= new Dictionary<(Type, ResourceFolder, string), List<ResourceInfo>>();
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
				ExecuteModScripts();
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
				await ExecuteModScriptsAsync();
		}

		public static void ExecuteModScripts()
		{
			foreach (Mod mod in mods)
			{
				mod.ExecuteScripts();
				ModLoaded?.Invoke(mod);
			}
		}

		public static async UniTask ExecuteModScriptsAsync()
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

		public static T Load<T>(ResourceFolder folder, string file) where T : class
		{
			var key = (typeof(T), folder, file);
			List<ResourceInfo> matchingResources = GetResources(key);
			if (matchingResources != null && matchingResources.Count > 0)
			{
				ResourceInfo matchingResource = matchingResources[0];
				ResourceReused?.Invoke(folder, file, matchingResource);
				return (T) matchingResource.Reference.Target;
			}

			string path = GetModdingPath(folder, file);
			foreach (Mod mod in mods)
			{
				var (reference, filePath, parser) = mod.Load<T>(path);
				if (reference != null)
				{
					if (matchingResources == null)
					{
						matchingResources = new List<ResourceInfo>();
						cachedResources[key] = matchingResources;
					}
					ResourceInfo newResource = new ResourceInfo(mod, filePath, parser, reference);
					matchingResources.Add(newResource);
					ResourceLoaded?.Invoke(folder, file, newResource);
					return reference;
				}
			}

			return null;
		}

		public static async UniTask<T> LoadAsync<T>(ResourceFolder folder, string file) where T : class
		{
			var key = (typeof(T), folder, file);
			List<ResourceInfo> matchingResources = GetResources(key);
			if (matchingResources != null && matchingResources.Count > 0)
			{
				ResourceInfo matchingResource = matchingResources[0];
				ResourceReused?.Invoke(folder, file, matchingResource);
				return (T) matchingResource.Reference.Target;
			}

			string path = GetModdingPath(folder, file);
			foreach (Mod mod in mods)
			{
				var (reference, filePath, parser) = await mod.LoadAsync<T>(path);
				if (reference != null)
				{
					if (matchingResources == null)
					{
						matchingResources = new List<ResourceInfo>();
						cachedResources[key] = matchingResources;
					}
					ResourceInfo newResource = new ResourceInfo(mod, filePath, parser, reference);
					matchingResources.Add(newResource);
					ResourceLoaded?.Invoke(folder, file, newResource);
					return reference;
				}
			}

			return null;
		}

		public static List<T> LoadAll<T>(ResourceFolder folder, string file) where T : class
		{
			var key = (typeof(T), folder, file);
			List<ResourceInfo> matchingResources = GetResources(key);
			// TODO: Maybe be incorrect
			Dictionary<Mod, ResourceInfo> matchingResourceByMod = matchingResources.ToDictionary(r => r.Mod);
			string path = GetModdingPath(folder, file);

			List<T> all = new List<T>();
			foreach (Mod mod in mods)
			{
				ResourceInfo matchingResource;
				if (!matchingResourceByMod.TryGetValue(mod, out matchingResource))
				{
					var (reference, filePath, parser) = mod.Load<T>(path);
					if (reference != null)
					{
						if (matchingResources == null)
						{
							matchingResources = new List<ResourceInfo>();
							cachedResources[key] = matchingResources;
						}
						ResourceInfo newResource = new ResourceInfo(mod, filePath, parser, reference);
						matchingResources.Add(newResource);
						ResourceLoaded?.Invoke(folder, file, newResource);
						all.Add(reference);
					}
				}
				else
				{
					ResourceReused?.Invoke(folder, file, matchingResource);
					all.Add((T) matchingResource.Reference.Target);
				}
			}
			return all;
		}

		public static async UniTask<List<T>> LoadAllAsync<T>(ResourceFolder folder, string file) where T : class
		{
			var key = (typeof(T), folder, file);
			List<ResourceInfo> matchingResources = GetResources(key);
			Dictionary<Mod, ResourceInfo> matchingResourceByMod = matchingResources.ToDictionary(r => r.Mod);
			string path = GetModdingPath(folder, file);

			List<T> all = new List<T>();
			foreach (Mod mod in mods)
			{
				ResourceInfo matchingResource;
				if (!matchingResourceByMod.TryGetValue(mod, out matchingResource))
				{
					var (reference, filePath, parser) = await mod.LoadAsync<T>(path);
					if (reference != null)
					{
						if (matchingResources == null)
						{
							matchingResources = new List<ResourceInfo>();
							cachedResources[key] = matchingResources;
						}
						ResourceInfo newResource = new ResourceInfo(mod, filePath, parser, reference);
						matchingResources.Add(newResource);
						ResourceLoaded?.Invoke(folder, file, newResource);
						all.Add(reference);
					}
				}
				else
				{
					ResourceReused?.Invoke(folder, file, matchingResource);
					all.Add((T) matchingResource.Reference.Target);
				}
			}
			return all;
		}
		
		public static List<ResourceInfo> GetResources<T>(ResourceFolder folder, string file)
		{
			return GetResources((typeof(T), folder, file));
		}

		public static ResourceInfo GetResourceInfo(object resource)
		{
			return cachedResources.SelectMany(r => r.Value).FirstOrDefault(r => r.Reference.Target == resource);
		}

		protected static List<ResourceInfo> GetResources((Type type, ResourceFolder folder, string file) key)
		{
			if (cachedResources.TryGetValue(key, out List<ResourceInfo> resources))
			{
				CleanupDeadResources(resources);
				return resources;
			}
			return null;
		}

		// HACK: This cleans up resources that have been garbage collected 
		protected static void CleanupDeadResources(List<ResourceInfo> list)
		{
			for (int i = list.Count - 1; i >= 0; i--)
				if (!list[i].Reference.IsAlive)
					list.RemoveAt(i);
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
				List<ResourceInfo> resourceList = kvp.Value;
				// FirstOrDefault will return "default" if not found and resource.Reference itself will be null
				ResourceInfo resource = resourceList.FirstOrDefault(r => r.Mod == mod);
				if (resource.Reference != null)
				{
					// No need to check for null because UnloadInternal checks type
					UnloadInternal(resource.Reference.Target);
					resourceList.Remove(resource);
					if (resourceList.Count <= 0)
						cachedResources.Remove(kvp.Key);
					found = true;
					ResourceUnloaded?.Invoke(kvp.Key.folder, kvp.Key.file, resource.Mod);
				}
			}
			return found;
		}

		public static bool Unload(object reference)
		{
			foreach (var kvp in cachedResources.Reverse())
			{
				// This differs from above in only that it returns immediately once a matching reference is found
				List<ResourceInfo> resourceList = kvp.Value;
				ResourceInfo resource = resourceList.FirstOrDefault(r => r.Reference.Target == reference);
				if (resource.Reference != null)
				{
					UnloadInternal(resource.Reference.Target);
					resourceList.Remove(resource);
					if (resourceList.Count <= 0)
						cachedResources.Remove(kvp.Key);
					ResourceUnloaded?.Invoke(kvp.Key.folder, kvp.Key.file, resource.Mod);
					return true;
				}
			}
			return false;
		}

		public static bool Unload<T>(ResourceFolder folder, string file)
		{
			var key = (typeof(T), folder, file);			
			List<ResourceInfo> matchingResources = GetResources(key);
			if (matchingResources != null)
			{
				foreach (ResourceInfo resource in matchingResources)
				{
					UnloadInternal(resource.Reference.Target);
					ResourceUnloaded?.Invoke(folder, file, resource.Mod);
				}
				cachedResources.Remove(key);
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