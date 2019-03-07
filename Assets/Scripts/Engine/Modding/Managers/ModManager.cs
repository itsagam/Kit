using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Modding.Parsers;
using UniRx;

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
	public enum ModType
	{
		Patch,
		Mod
	}

	public struct ModGroup
	{
		public ModType Name;
		public string Path;
		public List<Mod> Mods;
		public bool Deactivateable;
		public bool Reorderable;

		public ModGroup(ModType name, string path, bool deactivatable = true, bool reorderable = true)
		{
			Name = name;
			Path = path;
			Mods = new List<Mod>();
			Deactivateable = deactivatable;
			Reorderable = reorderable;
		}
	}

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

	public static class ModManager
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
		public static event Action<ResourceFolder, string, ResourceInfo, bool> ResourceLoaded;
		public static event Action<ResourceFolder, string, Mod> ResourceUnloaded;

		public static Dictionary<ModType, ModGroup> Groups = new Dictionary<ModType, ModGroup>();	
		public static List<ModLoader> Loaders = new List<ModLoader>() {
			new DirectModLoader(),
			new ZipModLoader()
		};
		public static IEnumerable<Mod> EnabledMods { get; private set; } = Enumerable.Empty<Mod>();

		private static Dictionary<(Type type, ResourceFolder folder, string file), ResourceInfo> cachedResources 
			= new Dictionary<(Type, ResourceFolder, string), ResourceInfo>();
		private static Dictionary<ResourceFolder, string> folderToString = new Dictionary<ResourceFolder, string>();

		#region Initialization
		static ModManager()
		{
			AddDefaultGroups();
			CacheFolderNames();
			Observable.OnceApplicationQuit().Subscribe(u => UnloadMods());
		}

		private static void AddDefaultGroups()
        {
			// The order in which groups are added is taken into account in mod order and cannot be changed by any means
			string writeableFolder = GetWriteableFolder();
			AddGroup(new ModGroup(ModType.Patch, writeableFolder + "Patches/", false, false));
			AddGroup(new ModGroup(ModType.Mod, writeableFolder + "Mods/", true, true));
		}

		private static string GetWriteableFolder()
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
		#endregion

		#region Mod-loading
		public static void LoadMods(bool executeScripts = false)
		{
			foreach (ModGroup group in Groups.Values)
			{
				if (!Directory.Exists(group.Path))
					continue;
				
				string[] childPaths = Directory.GetFileSystemEntries(group.Path);
				foreach (string childPath in childPaths)
				{
					foreach (ModLoader loader in Loaders)
					{
						Mod mod = loader.LoadMod(childPath);
						if (mod != null)
						{
							mod.Group = group;
							group.Mods.Add(mod);
							ModLoaded?.Invoke(mod);
							break;
						}
					}
				}
			}

			LoadModOrder();
			SaveModOrder();
			RefreshEnabledMods();

			if (executeScripts)
				ExecuteScripts();
		}

		public static async UniTask LoadModsAsync(bool executeScripts = false)
		{
			foreach (ModGroup group in Groups.Values)
			{
				if (!Directory.Exists(group.Path))
					continue;

				string[] childPaths = Directory.GetFileSystemEntries(group.Path);
				foreach (string childPath in childPaths)
				{
					foreach (ModLoader loader in Loaders)
					{
						Mod mod = await loader.LoadModAsync(childPath);
						if (mod != null)
						{
							mod.Group = group;
							group.Mods.Add(mod);
							ModLoaded?.Invoke(mod);
							break;
						}
					}
				}
			}

			LoadModOrder();
			SaveModOrder();
			RefreshEnabledMods();

			if (executeScripts)
				ExecuteScripts();
		}

		public static void ExecuteScripts()
		{
			foreach (Mod mod in EnabledMods)
				mod.ExecuteScripts();
		}

		public static async UniTask ExecuteScriptsAsync()
		{
			foreach (Mod mod in EnabledMods)
				await mod.ExecuteScriptsAsync();
		}

		private static void RefreshEnabledMods()
		{
			EnabledMods = Groups.SelectMany(kvp => kvp.Value.Mods).Where(m => IsModEnabled(m));
		}
		#endregion

		#region Settings
		public static void EnableMod(Mod mod)
		{
			ToggleMod(mod, true);
		}

		public static void DisableMod(Mod mod)
		{
			ToggleMod(mod, false);
		}

		public static void ToggleMod(Mod mod)
		{
			ToggleMod(mod, !IsModEnabled(mod));
		}

		public static void ToggleMod(Mod mod, bool value)
		{
			if (!mod.Group.Deactivateable)
				return;

			PlayerPrefs.SetInt($"{mod.Group.Name}/{mod.Metadata.Name}.Enabled", value ? 1 : 0);
			RefreshEnabledMods();
		}

		public static bool IsModEnabled(Mod mod)
		{
			if (!mod.Group.Deactivateable)
				return true;
			return PlayerPrefs.GetInt($"{mod.Group.Name}/{mod.Metadata.Name}.Enabled", 1) == 1;
		}

		public static int GetModOrder(Mod mod)
		{
			return EnabledMods.IndexOf(mod);
		}

		public static int GetModOrderInGroup(Mod mod)
		{
			return mod.Group.Mods.FindIndex(p => p == mod);
		}

		public static void MoveModTop(Mod mod)
		{
			MoveModOrder(mod, 0);
		}

		public static void MoveModBottom(Mod mod)
		{
			MoveModOrder(mod, mod.Group.Mods.Count-1);
		}

		public static void MoveModUp(Mod mod)
		{
			MoveModOrder(mod, GetModOrderInGroup(mod) - 1);
		}

		public static void MoveModDown(Mod mod)
		{
			MoveModOrder(mod, GetModOrderInGroup(mod) + 1);
		}

		public static void MoveModOrder(Mod mod, int index)
		{
			if (!mod.Group.Reorderable)
				return;

			if (index < 0 || index >= mod.Group.Mods.Count)
				return;

			mod.Group.Mods.Remove(mod);
			mod.Group.Mods.Insert(index, mod);
			SaveModOrder();
			RefreshEnabledMods();
		}

		private static void LoadModOrder()
		{
			foreach (var kvp in Groups)
			{
				ModGroup group = kvp.Value;
				if (group.Reorderable)
				{
					// Reversing the list makes sure new mods (whose entries we do not have and will have all the same value -1)
					// are ordered in reverse (newer on top)
					group.Mods = group.Mods.AsEnumerable()
						.Reverse()
						.OrderBy(m => PlayerPrefs.GetInt($"{group.Name}/{m.Metadata.Name}.Order", -1))
						.ToList();
				}
			}
		}

		private static void SaveModOrder()
		{
			foreach (ModGroup group in Groups.Values)
			{
				if (group.Reorderable)
				{
					for (int i = 0; i < group.Mods.Count; i++)
					{
						Mod mod = group.Mods[i];
						PlayerPrefs.SetInt($"{group.Name}/{mod.Metadata.Name}.Order", i);
					}
				}
			}	
		}
		#endregion

		#region Resource-loading
		// The "+" operator and Path.Combine are really costly and have a huge perfomance impact, thus this
		private static void CacheFolderNames()
		{
			foreach (ResourceFolder value in Enum.GetValues(typeof(ResourceFolder)))
				folderToString[value] = Enum.GetName(typeof(ResourceFolder), value) + "/";
		}

		private static object LoadCached(Type type, ResourceFolder folder, string file)
		{
			if (cachedResources.TryGetValue((type, folder, file), out ResourceInfo resource))
			{
				object reference = resource.Reference.Target;
				if (reference != null)
				{
					ResourceLoaded?.Invoke(folder, file, resource, false);
					return reference;
				}
			}
			return null;
		}

		public static object Load(Type type, ResourceFolder folder, string file)
		{
			object cachedReference = LoadCached(type, folder, file);
			if (cachedReference != null)
				return cachedReference;

			if (EnabledMods.Any())
			{
				string path = GetModdingPath(folder, file);
				foreach (Mod mod in EnabledMods)
				{
					var (reference, filePath, parser) = mod.Load(type, path);
					if (reference != null)
					{
						ResourceInfo resource = new ResourceInfo(mod, filePath, parser, reference);
						cachedResources[(type, folder, file)] = resource;
						ResourceLoaded?.Invoke(folder, file, resource, true);
						return reference;
					}
				}
			}

			return null;
		}

		public static async UniTask<object> LoadAsync(Type type, ResourceFolder folder, string file)
		{
			object cachedReference = LoadCached(type, folder, file);
			if (cachedReference != null)
				return cachedReference;

			if (EnabledMods.Any())
			{
				string path = GetModdingPath(folder, file);
				foreach (Mod mod in EnabledMods)
				{
					var (reference, filePath, parser) = await mod.LoadAsync(type, path);
					if (reference != null)
					{
						ResourceInfo resource = new ResourceInfo(mod, filePath, parser, reference);
						cachedResources[(type, folder, file)] = resource;
						ResourceLoaded?.Invoke(folder, file, resource, true);
						return reference;
					}
				}
			}

			return null;
		}

		public static List<object> LoadAll(Type type, ResourceFolder folder, string file)
		{
			return LoadAll(type, GetModdingPath(folder, file));
		}

		public static List<object> LoadAll(Type type, string path)
		{
			List<object> all = new List<object>();
			foreach (Mod mod in EnabledMods)
			{
				var (reference, filePath, parser) = mod.Load(type, path);
				if (reference != null)
					all.Add(reference);
			}
			return all;
		}

		public static UniTask<List<object>> LoadAllAsync(Type type, ResourceFolder folder, string file) 
		{
			return LoadAllAsync(type, GetModdingPath(folder, file));
		}

		public static async UniTask<List<object>> LoadAllAsync(Type type, string path)
		{
			List<object> all = new List<object>();
			foreach (Mod mod in EnabledMods)
			{
				var (reference, filePath, parser) = await mod.LoadAsync(type, path);
				if (reference != null)
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
		#endregion

		#region Reading
		public static string ReadText(ResourceFolder folder, string file)
		{
			return ReadText(GetModdingPath(folder, file));
		}

		public static string ReadText(string path)
		{
			foreach (Mod mod in EnabledMods)
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
			foreach (Mod mod in EnabledMods)
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
			foreach (Mod mod in EnabledMods)
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
			foreach (Mod mod in EnabledMods)
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
			foreach (Mod mod in EnabledMods)
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
			foreach (Mod mod in EnabledMods)
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
			foreach (Mod mod in EnabledMods)
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
			foreach (Mod mod in EnabledMods)
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
		#endregion

		#region Unloading
		public static void UnloadMods(bool withResources = false)
		{
			foreach (Mod mod in EnabledMods.Reverse())
				UnloadMod(mod, withResources);
		}

		public static void UnloadMod(Mod mod, bool withResources = true)
		{
			if (withResources)
				UnloadAll(mod);
			mod.Unload();
			mod.Group.Mods.Remove(mod);
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

		private static void UnloadInternal(object resource)
		{
			if (resource is UnityEngine.Object unityObject)
				GameObject.Destroy(unityObject);
		}
		#endregion

		#region Public methods
		public static void AddGroup(ModGroup group)
		{
			Groups.Add(group.Name, group);
		}

		public static void RemoveGroup(ModGroup group)
		{
			Groups.Remove(group.Name);
		}

		public static List<Mod> GetModsByGroup(ModType name)
		{
			if (Groups.TryGetValue(name, out ModGroup group))
				return group.Mods;
			return null;
		}

		public static string GetModdingPath(ResourceFolder folder)
		{
			return folderToString[folder];
		}

		public static string GetModdingPath(ResourceFolder folder, string file)
		{
			return GetModdingPath(folder) + file;
		}

		public static void ClearCache()
		{
			cachedResources.Clear();
		}

		public static IEnumerable<Mod> Mods
		{
			get
			{
				return Groups.SelectMany(g => g.Value.Mods);
			}
		}
		#endregion
#endif
	}
}