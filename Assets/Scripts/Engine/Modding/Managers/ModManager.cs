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
		public ResourceParser Parser;
		public object Reference;
		
		public ResourceInfo(Mod mod, ResourceParser parser, object reference)
		{
			Mod = mod;
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
		public static event Action<string, ResourceInfo> ResourceLoaded;
		public static event Action<string, ResourceInfo> ResourceReused;
		public static event Action<string, Mod> ResourceUnloaded;

		public const string DefaultModFolderName = "Mods";

		public static List<string> SearchPaths = new List<string>();
		public static List<ModLoader> Loaders = new List<ModLoader>() {
			new DirectModLoader(),
			new ZipModLoader()
		};
		
		protected static List<Mod> mods = new List<Mod>();
		protected static Dictionary<string, List<ResourceInfo>> resourceInfos = new Dictionary<string, List<ResourceInfo>>();

		static ModManager()
		{
			AddDefaultSearchPaths();
		}

		protected static void AddDefaultSearchPaths()
        {
			// TODO: Adding "Patches" and patching system
			string writeableFolder = GetWriteableFolder();
			SearchPaths.Add(writeableFolder + "/" + DefaultModFolderName + "/");
		}

		protected static string GetWriteableFolder()
		{
			switch (Application.platform)
			{
				case RuntimePlatform.WindowsEditor:
				case RuntimePlatform.OSXEditor:
				case RuntimePlatform.LinuxEditor:
				case RuntimePlatform.WindowsPlayer:
				case RuntimePlatform.LinuxPlayer:
					return Path.GetDirectoryName(Application.dataPath);
		
				case RuntimePlatform.IPhonePlayer:
				case RuntimePlatform.Android:
					return Application.persistentDataPath;
			}
			return null;
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

		// TODO: Save actual filenames
		public static T Load<T>(string path) where T : class
		{
			List<ResourceInfo> loadedResources = GetResourceInfo(path);
			if (loadedResources != null)
			{
				ResourceInfo loadedResource = loadedResources[0];
				ResourceReused?.Invoke(path, loadedResource);
				return (T) loadedResource.Reference;
			}

			foreach (Mod mod in mods)
			{
				var (reference, parser) = mod.Load<T>(path);
				if (reference != null)
				{
					if (!resourceInfos.TryGetValue(path, out loadedResources))
					{
						loadedResources = new List<ResourceInfo>();
						resourceInfos[path] = loadedResources;
					}
					ResourceInfo resourceInfo = new ResourceInfo(mod, parser, reference);
					loadedResources.Add(resourceInfo);
					ResourceLoaded?.Invoke(path, resourceInfo);
					return reference;
				}
			}

			return null;
		}

		public static async UniTask<T> LoadAsync<T>(string path) where T : class
		{
			List<ResourceInfo> loadedResources = GetResourceInfo(path);
			if (loadedResources != null)
			{
				ResourceInfo loadedResource = loadedResources[0];
				ResourceReused?.Invoke(path, loadedResource);
				return (T) loadedResource.Reference;
			}

			foreach (Mod mod in mods)
			{
				var (reference, parser) = await mod.LoadAsync<T>(path);
				if (reference != null)
				{
					if (!resourceInfos.TryGetValue(path, out loadedResources))
					{
						loadedResources = new List<ResourceInfo>();
						resourceInfos[path] = loadedResources;
					}
					ResourceInfo resourceInfo = new ResourceInfo(mod, parser, reference);
					loadedResources.Add(resourceInfo);
					ResourceLoaded?.Invoke(path, resourceInfo);
					return reference;
				}
			}

			return null;
		}

		public static List<T> LoadAll<T>(string path) where T : class
		{
			List<ResourceInfo> loadedResources = GetResourceInfo(path);
			if (loadedResources == null)
			{
				loadedResources = new List<ResourceInfo>();
				resourceInfos[path] = loadedResources;
			}
			List<T> all = new List<T>();
			foreach (Mod mod in mods)
			{
				ResourceInfo loadedResource = loadedResources.Find(r => r.Mod == mod);
				if (loadedResource.Reference == null)
				{
					var (reference, parser) =  mod.Load<T>(path);
					if (reference != null)
					{
						ResourceInfo resourceInfo = new ResourceInfo(mod, parser, reference);
						loadedResources.Add(resourceInfo);
						ResourceLoaded?.Invoke(path, resourceInfo);
						all.Add(reference);
					}
				}
				else
				{
					ResourceReused?.Invoke(path, loadedResource);
					all.Add((T) loadedResource.Reference);
				}
			}
			return all;
		}

		public static async UniTask<List<T>> LoadAllAsync<T>(string path) where T : class
		{
			List<ResourceInfo> loadedResources = GetResourceInfo(path);
			if (loadedResources == null)
			{
				loadedResources = new List<ResourceInfo>();
				resourceInfos[path] = loadedResources;
			}
			List<T> all = new List<T>();
			foreach (Mod mod in mods)
			{
				ResourceInfo loadedResource = loadedResources.Find(r => r.Mod == mod);
				if (loadedResource.Reference == null)
				{
					var (reference, parser) = await mod.LoadAsync<T>(path);
					if (reference != null)
					{
						ResourceInfo resourceInfo = new ResourceInfo(mod, parser, reference);
						loadedResources.Add(resourceInfo);
						ResourceLoaded?.Invoke(path, resourceInfo);
						all.Add(reference);
					}
				}
				else
				{
					ResourceReused?.Invoke(path, loadedResource);
					all.Add((T) loadedResource.Reference);
				}
			}
			return all;
		}

		public static List<ResourceInfo> GetResourceInfo(string path)
		{
			if (resourceInfos.TryGetValue(path, out List<ResourceInfo> resourceInfo))
				if (resourceInfo.Count > 0)
					return resourceInfo;
			return null;
		}

		public static ResourceInfo? GetResourceInfo(object resource)
		{
			return resourceInfos.SelectMany(r => r.Value).FirstOrDefault(r => r.Reference == resource);
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
			Unload(o => o.Mod == mod);
			mod.Unload();
			mods.Remove(mod);
			ModUnloaded?.Invoke(mod);
		}

		public static bool Unload(object reference)
		{
			return Unload(o => o.Reference == reference);
		}

		public static bool Unload(Func<ResourceInfo, bool> predicate)
		{
			bool found = false;
			foreach (var kvp in resourceInfos.Reverse())
			{
				foreach (var info in kvp.Value.Where(predicate).Reverse())
				{
					UnloadInternal(info.Reference);
					kvp.Value.Remove(info);
					ResourceUnloaded?.Invoke(kvp.Key, info.Mod);
					found = true;
				}
				if (kvp.Value.Count <= 0)
					resourceInfos.Remove(kvp.Key);
			}
			return found;
		}

		public static bool UnloadAll(string path)
		{
			List<ResourceInfo> infos = GetResourceInfo(path);
			if (infos != null)
			{
				foreach (ResourceInfo info in infos)
				{
					UnloadInternal(info.Reference);
					ResourceUnloaded?.Invoke(path, info.Mod);
				}
				resourceInfos.Remove(path);
				return true;
			}
			return false;
		}

		protected static void UnloadInternal(object resource)
		{
			if (resource is UnityEngine.Object)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)resource);
			}
		}

		public static void ClearCache()
		{
			resourceInfos.Clear();
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