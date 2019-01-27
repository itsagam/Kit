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
	#if MODDING
	public struct ResourceInfo
	{
		public Mod Mod;
		public string File; // Actual filename, maybe different from the one resource was loaded with if extension was not provided
		public ResourceParser Parser;
		public WeakReference Reference;
		
		public ResourceInfo(Mod mod, string file, ResourceParser parser, object reference)
		{
			Mod = mod;
			File = file;
			Parser = parser;
			Reference = new WeakReference(reference);
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
		public static event Action<string, ResourceInfo> ResourceLoaded;
		public static event Action<string, ResourceInfo> ResourceReused;
		public static event Action<string, Mod> ResourceUnloaded;

		public const string DefaultModFolderName = "Mods";

		public static List<string> SearchPaths = new List<string>();
		public static List<ModLoader> Loaders = new List<ModLoader>() {
			//UNDONE: new AssetBundleLoader(),
			new DirectModLoader(),
			new ZipModLoader()
		};
		
		protected static List<Mod> mods = new List<Mod>();
		protected static Dictionary<string, List<ResourceInfo>> cachedResources = new Dictionary<string, List<ResourceInfo>>();

		static ModManager()
		{
			AddDefaultSearchPaths();
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

		public static void LoadMods()
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

			foreach (Mod mod in mods)
			{
				mod.ExecuteScripts();
				ModLoaded?.Invoke(mod);
			}
		}

		public static async UniTask LoadModsAsync()
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

			foreach (Mod mod in mods)
			{
				await mod.ExecuteScriptsAsync();
				ModLoaded?.Invoke(mod);
			}
		}

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

		public static T Load<T>(string path) where T : class
		{
			List<ResourceInfo> matchingResources = GetResources(path);
			if (matchingResources != null && matchingResources.Count > 0)
			{
				ResourceInfo matchingResource = matchingResources[0];
				ResourceReused?.Invoke(path, matchingResource);
				return (T) matchingResource.Reference.Target;
			}

			foreach (Mod mod in mods)
			{
				var (reference, file, parser) = mod.Load<T>(path);
				if (reference != null)
				{
					if (matchingResources == null)
					{
						matchingResources = new List<ResourceInfo>();
						cachedResources[path] = matchingResources;
					}
					ResourceInfo newResource = new ResourceInfo(mod, file, parser, reference);
					matchingResources.Add(newResource);
					ResourceLoaded?.Invoke(path, newResource);
					return reference;
				}
			}

			return null;
		}

		public static async UniTask<T> LoadAsync<T>(string path) where T : class
		{
			List<ResourceInfo> matchingResources = GetResources(path);
			if (matchingResources != null && matchingResources.Count > 0)
			{
				ResourceInfo matchingResource = matchingResources[0];
				ResourceReused?.Invoke(path, matchingResource);
				return (T) matchingResource.Reference.Target;
			}

			foreach (Mod mod in mods)
			{
				var (reference, file, parser) = await mod.LoadAsync<T>(path);
				if (reference != null)
				{
					if (matchingResources == null)
					{
						matchingResources = new List<ResourceInfo>();
						cachedResources[path] = matchingResources;
					}
					ResourceInfo newResource = new ResourceInfo(mod, file, parser, reference);
					matchingResources.Add(newResource);
					ResourceLoaded?.Invoke(path, newResource);
					return reference;
				}
			}

			return null;
		}

		public static List<T> LoadAll<T>(string path) where T : class
		{
			List<ResourceInfo> matchingResources = GetResources(path);
			Dictionary<Mod, ResourceInfo> matchingByMod = matchingResources.ToDictionary(r => r.Mod);
			List<T> all = new List<T>();
			foreach (Mod mod in mods)
			{
				ResourceInfo matchingResource;
				if (!matchingByMod.TryGetValue(mod, out matchingResource))
				{
					var (reference, file, parser) =  mod.Load<T>(path);
					if (reference != null)
					{
						if (matchingResources == null)
						{
							matchingResources = new List<ResourceInfo>();
							cachedResources[path] = matchingResources;
						}
						ResourceInfo newResource = new ResourceInfo(mod, file, parser, reference);
						matchingResources.Add(newResource);
						ResourceLoaded?.Invoke(path, newResource);
						all.Add(reference);
					}
				}
				else
				{
					ResourceReused?.Invoke(path, matchingResource);
					all.Add((T) matchingResource.Reference.Target);
				}
			}
			return all;
		}

		public static async UniTask<List<T>> LoadAllAsync<T>(string path) where T : class
		{
			List<ResourceInfo> matchingResources = GetResources(path);
			Dictionary<Mod, ResourceInfo> matchingByMod = matchingResources.ToDictionary(r => r.Mod);
			List<T> all = new List<T>();
			foreach (Mod mod in mods)
			{
				ResourceInfo matchingResource;
				if (!matchingByMod.TryGetValue(mod, out matchingResource))
				{
					var (reference, file, parser) = await mod.LoadAsync<T>(path);
					if (reference != null)
					{
						if (matchingResources == null)
						{
							matchingResources = new List<ResourceInfo>();
							cachedResources[path] = matchingResources;
						}
						ResourceInfo newResource = new ResourceInfo(mod, file, parser, reference);
						matchingResources.Add(newResource);
						ResourceLoaded?.Invoke(path, newResource);
						all.Add(reference);
					}
				}
				else
				{
					ResourceReused?.Invoke(path, matchingResource);
					all.Add((T) matchingResource.Reference.Target);
				}
			}
			return all;
		}

		public static List<ResourceInfo> GetResources(string path)
		{
			if (cachedResources.TryGetValue(path, out List<ResourceInfo> resources))
			{
				CleanupDeadResources(resources);
				return resources;
			}
			return null;
		}

		protected static void CleanupDeadResources(List<ResourceInfo> list)
		{
			for (int i = list.Count - 1; i >= 0; i--)
				if (!list[i].Reference.IsAlive)
					list.RemoveAt(i);
		}

		public static ResourceInfo? GetResourceInfo(object resource)
		{
			return cachedResources.SelectMany(r => r.Value).FirstOrDefault(r => r.Reference.Target == resource);
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

		public static void ReloadMods()
        {
            UnloadMods();
            LoadMods();
        }

        public static void UnloadMods()
		{
			for (int i = mods.Count - 1; i >= 0; i--)
				UnloadMod(mods[i]);
		}

		public static void UnloadMod(Mod mod)
		{
			UnloadAll(o => o.Mod == mod);
			mod.Unload();
			mods.Remove(mod);
			ModUnloaded?.Invoke(mod);
		}

		public static bool Unload(object reference)
		{
			return Unload(o => o.Reference.Target == reference);
		}

		public static bool Unload(Func<ResourceInfo, bool> predicate)
		{
			foreach (var kvp in cachedResources.Reverse())
			{
				foreach (var resource in kvp.Value.Where(predicate).Reverse())
				{
					UnloadInternal(resource.Reference.Target);
					kvp.Value.Remove(resource);
					ResourceUnloaded?.Invoke(kvp.Key, resource.Mod);
					if (kvp.Value.Count <= 0)
						cachedResources.Remove(kvp.Key);
					return true;
				}
			}
			return false;
		}

		public static bool UnloadAll(Func<ResourceInfo, bool> predicate)
		{
			bool found = false;
			foreach (var kvp in cachedResources.Reverse())
			{
				foreach (var resource in kvp.Value.Where(predicate).Reverse())
				{
					UnloadInternal(resource.Reference.Target);
					kvp.Value.Remove(resource);
					ResourceUnloaded?.Invoke(kvp.Key, resource.Mod);
					found = true;
				}
				if (kvp.Value.Count <= 0)
					cachedResources.Remove(kvp.Key);
			}
			return found;
		}

		public static bool UnloadAll(string path)
		{
			List<ResourceInfo> matchingResources = GetResources(path);
			if (matchingResources != null)
			{
				foreach (ResourceInfo resource in matchingResources)
				{
					UnloadInternal(resource.Reference.Target);
					ResourceUnloaded?.Invoke(path, resource.Mod);
				}
				cachedResources.Remove(path);
				return true;
			}
			return false;
		}

		protected static void UnloadInternal(object resource)
		{
			if (resource is UnityEngine.Object)
				UnityEngine.Object.Destroy((UnityEngine.Object)resource);
		}

		public static void ClearCache()
		{
			cachedResources.Clear();
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