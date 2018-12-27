﻿using System;
using System.IO;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Modding
{
	public class ResourceInfo
	{
		public ModPackage Package;
		public UnityEngine.Object Reference;

		public ResourceInfo(ModPackage package, UnityEngine.Object reference)
		{
			Package = package;
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

        public static List<ModLoader> Loaders = new List<ModLoader>();
		public static List<ModParser> Parsers = new List<ModParser>();
		public static List<string> SearchPaths = new List<string>();

		protected static List<ModPackage> modPackages = new List<ModPackage>();
		protected static Dictionary<string, List<ResourceInfo>> resourceInfos = new Dictionary<string, List<ResourceInfo>>();

		static ModManager()
		{
			AddDefaultLoaders();
			AddDefaultParsers();
			AddDefaultSearchPaths();
		}

		private static void AddDefaultLoaders()
        {
			Loaders.Add(new Loaders.DirectModLoader());
			Loaders.Add(new Loaders.ZipModLoader());
		}

		private static void AddDefaultParsers()
		{
			Parsers.Add(new Parsers.TextureParser());
			Parsers.Add(new Parsers.AudioParser());
			Parsers.Add(new Parsers.TextParser());
		}

		private static void AddDefaultSearchPaths()
        {
            SearchPaths.Add(Application.streamingAssetsPath);
        }

		public static void LoadMods()
		{
			foreach (string path in SearchPaths)
			{
				string[] childPaths = Directory.GetFileSystemEntries(path);
				foreach (string childPath in childPaths)
				{
					foreach (ModLoader loader in Loaders)
					{
						ModPackage package = loader.LoadMod(childPath);
						if (package != null)
						{
							modPackages.Add(package);
							if (ModLoaded != null)
								ModLoaded(package);
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

		public static T Load<T>(string path) where T : UnityEngine.Object
		{
			return LoadInternal<T>(path, false).Result;
		}

		public static async Task<T> LoadAsync<T>(string path) where T : UnityEngine.Object
		{
			return await LoadInternal<T>(path, true);
		}

		protected static async Task<T> LoadInternal<T>(string path, bool async) where T : UnityEngine.Object
        {
			List<ResourceInfo> resourcesInfos = GetResourceInfo(path);
			if (resourcesInfos != null)
			{
				ResourceInfo resourceInfo = resourcesInfos[0];
				if (ResourceReused != null)
					ResourceReused(path, resourceInfo);
				return resourceInfo.Reference as T;
			}

			foreach (ModPackage package in ModPackages.Reverse())
			{
				T reference = async ? await package.LoadAsync<T>(path) : package.Load<T>(path);
				if (reference != null)
				{
					ResourceInfo resourceInfo = new ResourceInfo(package, reference);
					if (!resourceInfos.ContainsKey(path))
						resourceInfos[path] = new List<ResourceInfo>();
					resourceInfos[path].Add(resourceInfo);

					if (ResourceLoaded != null)
						ResourceLoaded(path, resourceInfo);

					return reference;
				}
			}
			return null;
        }

		public static List<T> LoadAll<T>(string path) where T : UnityEngine.Object
		{
			return LoadAllInternal<T>(path, false).Result;
		}

		public static async Task<List<T>> LoadAllAsync<T>(string path) where T : UnityEngine.Object
		{
			return await LoadAllInternal<T>(path, true);
		}

		protected static async Task<List<T>> LoadAllInternal<T>(string path, bool async) where T : UnityEngine.Object
		{
			List<T> all = new List<T>();
			List<ResourceInfo> cachedInfos = GetResourceInfo(path);
			foreach (ModPackage package in ModPackages)
			{
				ResourceInfo cachedInfo = cachedInfos?.FirstOrDefault(i => i.Package == package);
				if (cachedInfo == null)
				{
					T reference = async ? await package.LoadAsync<T>(path) : package.Load<T>(path);
					if (reference != null)
					{
						ResourceInfo resourceInfo = new ResourceInfo(package, reference);
						if (!resourceInfos.ContainsKey(path))
							resourceInfos[path] = new List<ResourceInfo>();
						resourceInfos[path].Add(resourceInfo);

						if (ResourceLoaded != null)
							ResourceLoaded(path, resourceInfo);

						all.Add(reference);
					}
				}
				else
				{
					if (ResourceReused != null)
						ResourceReused(path, cachedInfo);
					all.Add(cachedInfo.Reference as T);
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

		public static byte[] ReadBytes(string path, bool async)
		{
			return ReadBytesInternal(path, false).Result;
		}

		public static async Task<byte[]> ReadBytesAsync(string path, bool async)
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
			if (ModUnloaded != null)
				ModUnloaded(package);
		}

		public static bool Unload(UnityEngine.Object obj)
		{
			return Unload(i => i.Reference == obj);
		}

		public static bool Unload(Func<ResourceInfo, bool> predicate)
		{
			bool found = false;
			foreach (var kvp in resourceInfos)
			{
				foreach (var info in kvp.Value.Where(predicate).Reverse())
				{
					kvp.Value.Remove(info);
					if (ResourceUnloaded != null)
						ResourceUnloaded(kvp.Key, info.Package);
					found = true;
				}
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
					if (ResourceUnloaded != null)
						ResourceUnloaded(path, info.Package);
				}
				resourceInfos.Remove(path);
				return true;
			}
			return false;
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