using System;
using System.IO;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Modding.Loaders;
using Modding.Parsers;

namespace Modding
{
	public struct ResourceInfo
	{
		public ModPackage Package;
		public ResourceParser Parser;
		public object Reference;
		
		public ResourceInfo(ModPackage package, ResourceParser parser, object reference)
		{
			Package = package;
			Parser = parser;
			Reference = reference;
		}
	}

	public class ModManager
	{
		public delegate void ModLoadedHandler(ModPackage modPackage);
		public delegate void ModUnloadedHandler(ModPackage modPackage);
		public delegate void ResourceLoadedHandler(string path, ResourceInfo info);
		public delegate void ResourceReusedHandler(string path, ResourceInfo info);
		public delegate void ResourceUnloadedHandler(string path, ModPackage package);

		public static event ModLoadedHandler ModLoaded;
		public static event ModUnloadedHandler ModUnloaded;
		public static event ResourceLoadedHandler ResourceLoaded;
		public static event ResourceReusedHandler ResourceReused;
		public static event ResourceUnloadedHandler ResourceUnloaded;

		public const string DefaultSearchFolderName = "Mods";

        public static List<ModLoader> Loaders = new List<ModLoader>();
		public static List<ResourceParser> Parsers = new List<ResourceParser>();
		public static List<string> SearchPaths = new List<string>();

		protected static List<ModPackage> modPackages = new List<ModPackage>();
		protected static Dictionary<string, List<ResourceInfo>> resourceInfos = new Dictionary<string, List<ResourceInfo>>(StringComparer.OrdinalIgnoreCase);

		static ModManager()
		{
			AddDefaultLoaders();
			AddDefaultParsers();
			AddDefaultSearchPaths();
		}

		private static void AddDefaultLoaders()
        {
			Loaders.Add(new DirectModLoader());
			Loaders.Add(new ZipModLoader());
			// TODO: Loaders.Add(new AssetBundleModLoader());
		}

		private static void AddDefaultParsers()
		{
			Parsers.Add(new JSONParser());

			Parsers.Add(new TextureParser());
			Parsers.Add(new AudioParser());
			Parsers.Add(new TextParser());
		}

		private static void AddDefaultSearchPaths()
        {
			switch (Application.platform)
			{
				case RuntimePlatform.WindowsEditor:
				case RuntimePlatform.OSXEditor:
				case RuntimePlatform.LinuxEditor:

				case RuntimePlatform.WindowsPlayer:
				case RuntimePlatform.LinuxPlayer:
					SearchPaths.Add(Path.Combine(Path.GetDirectoryName(Application.dataPath), DefaultSearchFolderName));
					break;

				case RuntimePlatform.IPhonePlayer:
				case RuntimePlatform.Android:
					SearchPaths.Add(Path.Combine(Application.persistentDataPath, DefaultSearchFolderName));
					break;
			}
		}

		public static void LoadMods()
		{
			LoadModsInternal(false).Wait();
		}

		public static async Task LoadModsAsync()
		{
			await LoadModsInternal(true); 
		}

		protected static async Task LoadModsInternal(bool async)
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
						ModPackage package = async ? await loader.LoadModAsync(childPath) : loader.LoadMod(childPath);
						if (package != null)
						{
							modPackages.Add(package);
							ModLoaded?.Invoke(package);
							break;
						}
					}
				}
			}

			for (int i = modPackages.Count-1; i>=0; i--)
			{
				string key = modPackages[i].Metadata.Name;
				if (PlayerPrefs.HasKey(key))
					MoveModOrder(modPackages[i], PlayerPrefs.GetInt(key));
			}

			SaveModOrder();
		}
	
		public static int GetModOrder(ModPackage modPackage)
		{
			return modPackages.FindIndex(p => p == modPackage);
		}

		public static void MoveModOrder(ModPackage modPackage, int index)
		{
			modPackages.Remove(modPackage);
			modPackages.Insert(index, modPackage);
		}

		public static void SaveModOrder()
		{
			for (int i = 0; i < modPackages.Count; i++)
				PlayerPrefs.SetInt(modPackages[i].Metadata.Name, i);
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

		public static T Load<T>(string path) where T : class
		{
			return LoadInternal<T>(path, false).Result;
		}

		public static async Task<T> LoadAsync<T>(string path) where T : class
		{
			return await LoadInternal<T>(path, true);
		}

		protected static async Task<T> LoadInternal<T>(string path, bool async) where T : class
		{
			List<ResourceInfo> loadedResources = GetResourceInfo(path);
			if (loadedResources != null)
			{
				ResourceInfo loadedResource = loadedResources[0];
				ResourceReused?.Invoke(path, loadedResource);
				return (T) loadedResource.Reference;
			}
			
			foreach (ModPackage package in ModPackages.Reverse())
			{
				var (reference, parser) = async ? await package.LoadAsync<T>(path) : package.Load<T>(path);
				if (reference != null)
				{
					if (! resourceInfos.TryGetValue(path, out loadedResources))
					{
						loadedResources = new List<ResourceInfo>();
						resourceInfos[path] = loadedResources;
					}
					ResourceInfo resourceInfo = new ResourceInfo(package, parser, reference);
					loadedResources.Add(resourceInfo);
					ResourceLoaded?.Invoke(path, resourceInfo);
					return reference;
				}
			}
			
			return null;
        }

		public static List<T> LoadAll<T>(string path) where T : class
		{
			return LoadAllInternal<T>(path, false).Result;
		}

		public static async Task<List<T>> LoadAllAsync<T>(string path) where T : class
		{
			return await LoadAllInternal<T>(path, true);
		}

		protected static async Task<List<T>> LoadAllInternal<T>(string path, bool async) where T : class
		{
			List<T> all = new List<T>();
			List<ResourceInfo> loadedResources = GetResourceInfo(path);
			if (loadedResources == null)
			{
				loadedResources = new List<ResourceInfo>();
				resourceInfos[path] = loadedResources;
			}
			foreach (ModPackage package in ModPackages)
			{
				ResourceInfo loadedResource = loadedResources.Find(r => r.Package == package);
				if (loadedResource.Reference == null)
				{
					var (reference, parser) = async ? await package.LoadAsync<T>(path) : package.Load<T>(path);
					if (reference != null)
					{
						ResourceInfo resourceInfo = new ResourceInfo(package, parser, reference);
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

		public static string ReadText(string path)
		{
			return ReadTextInternal(path, false).Result;
		}

		public static async Task<string> ReadTextAsync(string path)
		{
			return await ReadTextInternal(path, true);
		}

		protected static async Task<string> ReadTextInternal(string path, bool async)
		{
			foreach (ModPackage package in ModPackages.Reverse())
			{
				try
				{
					if (async)
						return await package.ReadTextAsync(path);
					else
						return package.ReadText(path);
				}
				catch (FileNotFoundException)
				{
				}
			}
			return null;
		}

		public static List<string> ReadTextAll(string path)
		{
			return ReadTextAllInternal(path, false).Result;
		}

		public static async Task<List<string>> ReadTextAllAsync(string path)
		{
			return await ReadTextAllInternal(path, true);
		}

		protected static async Task<List<string>> ReadTextAllInternal(string path, bool async)
		{
			List<string> all = new List<string>();

			foreach (ModPackage package in ModPackages)
			{
				try
				{
					if (async)
						all.Add(await package.ReadTextAsync(path));
					else
						all.Add(package.ReadText(path));
				}
				catch (FileNotFoundException)
				{
				}
			}
			
			return all;
		}

		public static byte[] ReadBytes(string path)
		{
			return ReadBytesInternal(path, false).Result;
		}

		public static async Task<byte[]> ReadBytesAsync(string path)
		{
			return await ReadBytesInternal(path, true);
		}

		protected static async Task<byte[]> ReadBytesInternal(string path, bool async)
		{
			foreach (ModPackage package in ModPackages.Reverse())
			{
				try
				{
					if (async)
						return await package.ReadBytesAsync(path);
					else
						return package.ReadBytes(path);
				}
				catch (FileNotFoundException)
				{
				}
			}
			return null;
		}

		public static List<byte[]> ReadBytesAll(string path)
		{
			return ReadBytesAllInternal(path, false).Result;
		}

		public static async Task<List<byte[]>> ReadBytesAllAsync(string path)
		{
			return await ReadBytesAllInternal(path, true);
		}

		protected static async Task<List<byte[]>> ReadBytesAllInternal(string path, bool async)
		{
			List<byte[]> all = new List<byte[]>();
			foreach (ModPackage package in ModPackages)
			{
				try
				{
					if (async)
						all.Add(await package.ReadBytesAsync(path));
					else
						all.Add(package.ReadBytes(path));
				}
				catch (FileNotFoundException)
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
			for (int i = modPackages.Count - 1; i >= 0; i--)
				UnloadMod(modPackages[i]);
		}

		public static void UnloadMod(ModPackage package)
		{
			Unload(i => i.Package == package);
			package.Unload();
			modPackages.Remove(package);
			ModUnloaded?.Invoke(package);
		}

		public static bool Unload(object obj)
		{
			return Unload(i => i.Reference == obj);
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
					ResourceUnloaded?.Invoke(kvp.Key, info.Package);
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
					ResourceUnloaded?.Invoke(path, info.Package);
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

		public static ReadOnlyCollection<ModPackage> ModPackages
		{
			get
			{
				return modPackages.AsReadOnly();
			}
		}
	}
}