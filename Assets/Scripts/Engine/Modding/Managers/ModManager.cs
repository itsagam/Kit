#if MODDING
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Engine.Modding.Loaders;
using UniRx;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Engine.Modding
{
	public static class ModManager
	{
		#region Fields
		public static event Action<Mod> ModLoaded;
		public static event Action<Mod> ModUnloaded;
		public static event Action<ResourceFolder, string, ResourceInfo, bool> ResourceLoaded;
		public static event Action<ResourceFolder, string, Mod> ResourceUnloaded;

		public static readonly Dictionary<ModType, ModGroup> Groups = new Dictionary<ModType, ModGroup>();
		public static readonly List<ModLoader> Loaders = new List<ModLoader>{
				new DirectModLoader(),
				new ZipModLoader(),
				new AssetBundleModLoader()
		};
		public static IReadOnlyList<Mod> ActiveMods { get; private set; } = new List<Mod>();

		private static Dictionary<(Type type, ResourceFolder folder, string file), ResourceInfo> cachedResources =
				new Dictionary<(Type, ResourceFolder, string), ResourceInfo>();
		private static Dictionary<ResourceFolder, string> folderToString = new Dictionary<ResourceFolder, string>();
		#endregion

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
			string writableFolder = GetWritableFolder();
			AddGroup(new ModGroup(ModType.Patch, writableFolder + "Patches/", false, false));
			AddGroup(new ModGroup(ModType.Mod, writableFolder + "Mods/", true, true));
		}

		public static string GetWritableFolder()
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

		// The "+" operator and Path.Combine are really costly and have a huge performance impact, thus this
		private static void CacheFolderNames()
		{
			foreach (ResourceFolder value in Enum.GetValues(typeof(ResourceFolder)))
				folderToString[value] = Enum.GetName(typeof(ResourceFolder), value) + "/";
		}
		#endregion

		#region Mod-loading
		public static Dictionary<ModGroup, string[]> GetModPathsByGroup()
		{
			return Groups.Values
			             .Where(g => Directory.Exists(g.Path))
						 .ToDictionary(g => g, g => Directory.GetFileSystemEntries(g.Path));
		}

		public static void LoadMods(bool executeScripts = false)
		{
			LoadMods(GetModPathsByGroup(), executeScripts);
		}

		public static void LoadMods(Dictionary<ModGroup, string[]> modPaths, bool executeScripts = false)
		{
			UnloadMods(false);

			foreach ((ModGroup group, var childPaths) in modPaths)
				foreach (string childPath in childPaths)
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

			LoadModOrder();
			SaveModOrder();
			RefreshActiveMods();

			Debugger.Log("ModManager", $"{Mods.Count()} mods loaded, {ActiveMods.Count} active.");

			if (executeScripts)
				ExecuteScripts();
		}

		public static UniTask LoadModsAsync(bool executeScripts = false)
		{
			return LoadModsAsync(GetModPathsByGroup(), executeScripts);
		}

		public static async UniTask LoadModsAsync(Dictionary<ModGroup, string[]> modPaths, bool executeScripts = false)
		{
			UnloadMods(false);

			foreach ((ModGroup group, var childPaths) in modPaths)
				foreach (string childPath in childPaths)
					foreach (ModLoader loader in Loaders)
					{
						Mod mod = await loader.LoadModAsync(childPath);
						if (mod != null)
						{
							mod.Group = group;
							group.Mods.Add(mod);
							ModLoaded?.Invoke(mod);
							Debugger.Log("ModManager", $"Loaded mod \"{mod.Metadata.Name}\".");
							break;
						}
					}

			LoadModOrder();
			SaveModOrder();
			RefreshActiveMods();

			Debugger.Log("ModManager",$"{Mods.Count()} mods loaded, {ActiveMods.Count} active.");

			if (executeScripts)
				await ExecuteScriptsAsync();
		}

		public static void ExecuteScripts()
		{
			foreach (Mod mod in ActiveMods)
				mod.ExecuteScripts();
		}

		public static async UniTask ExecuteScriptsAsync()
		{
			foreach (Mod mod in ActiveMods)
				await mod.ExecuteScriptsAsync();
		}

		private static void RefreshActiveMods()
		{
			ActiveMods = Groups.SelectMany(kvp => kvp.Value.Mods).Where(IsModEnabled).ToList();
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
			if (!mod.Group.Deactivatable)
				return;

			SettingsManager.Set(mod.Group.Name.ToString(), mod.Metadata.Name, "Enabled", value);
			RefreshActiveMods();
		}

		public static bool IsModEnabled(Mod mod)
		{
			if (!mod.Group.Deactivatable)
				return true;

			return SettingsManager.Get(mod.Group.Name.ToString(), mod.Metadata.Name, "Enabled", true);
		}

		public static int GetModOrder(Mod mod)
		{
			return ActiveMods.IndexOf(mod);
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
			RefreshActiveMods();
		}

		private static void LoadModOrder()
		{
			// Reversing the list makes sure new mods (whose entries we do not have and will have all the same value -1)
			// are ordered in reverse (newer on top)
			foreach (ModGroup group in Groups.Values)
				if (group.Reorderable)
					group.Mods = group.Mods.AsEnumerable()
									  .Reverse()
									  .OrderBy(mod => SettingsManager.Get(group.Name.ToString(),
																			 mod.Metadata.Name,
																			 "Order", -1))
									  .ToList();
		}

		private static void SaveModOrder()
		{
			foreach (ModGroup group in Groups.Values)
				if (group.Reorderable)
					for (int order = 0; order < group.Mods.Count; order++)
					{
						Mod mod = group.Mods[order];
						SettingsManager.Set(group.Name.ToString(), mod.Metadata.Name, "Order", order);
					}
		}

		#endregion

		#region Resource-loading
		private static object LoadCached(Type type, ResourceFolder folder, string file)
		{
			if (!cachedResources.TryGetValue((type, folder, file), out ResourceInfo resource))
				return null;

			object reference = resource.Reference.Target;
			if (reference == null)
				return null;

			ResourceLoaded?.Invoke(folder, file, resource, false);
			return reference;
		}

		public static object Load<T>(ResourceFolder folder, string file)
		{
			return (T) Load(typeof(T), folder, file);
		}

		public static object Load(Type type, ResourceFolder folder, string file)
		{
			object cachedReference = LoadCached(type, folder, file);
			if (cachedReference != null)
				return cachedReference;

			if (ActiveMods.Count <= 0)
				return null;

			string path = GetModdingPath(folder, file);
			foreach (Mod mod in ActiveMods)
			{
				var (reference, filePath, parser) = mod.LoadEx(type, path);
				if (reference != null)
				{
					ResourceInfo resource = new ResourceInfo(mod, filePath, parser, reference);
					cachedResources[(type, folder, file)] = resource;
					ResourceLoaded?.Invoke(folder, file, resource, true);
					return reference;
				}
			}

			return null;
		}

		public static async UniTask<T> LoadAsync<T>(ResourceFolder folder, string file)
		{
			return (T) await LoadAsync(typeof(T), folder, file);
		}

		public static async UniTask<object> LoadAsync(Type type, ResourceFolder folder, string file)
		{
			object cachedReference = LoadCached(type, folder, file);
			if (cachedReference != null)
				return cachedReference;

			if (ActiveMods.Count <= 0)
				return null;

			string path = GetModdingPath(folder, file);
			foreach (Mod mod in ActiveMods)
			{
				var (reference, filePath, parser) = await mod.LoadExAsync(type, path);
				if (reference != null)
				{
					ResourceInfo resource = new ResourceInfo(mod, filePath, parser, reference);
					cachedResources[(type, folder, file)] = resource;
					ResourceLoaded?.Invoke(folder, file, resource, true);
					return reference;
				}
			}

			return null;
		}

		public static IEnumerable<T> LoadAll<T>(ResourceFolder folder, string file)
		{
			return LoadAll<T>(GetModdingPath(folder, file));
		}

		public static IEnumerable<object> LoadAll(Type type, ResourceFolder folder, string file)
		{
			return LoadAll(type, GetModdingPath(folder, file));
		}

		public static IEnumerable<T> LoadAll<T>(string path)
		{
			return ActiveMods.Select(mod => (T) mod.LoadEx(typeof(T), path).reference).Where(b => b != null);
		}

		public static IEnumerable<object> LoadAll(Type type, string path)
		{
			return ActiveMods.Select(mod => mod.LoadEx(type, path).reference).Where(b => b != null);
		}

		public static UniTask<IEnumerable<T>> LoadAllAsync<T>(ResourceFolder folder, string file)
		{
			return LoadAllAsync<T>(GetModdingPath(folder, file));
		}

		public static UniTask<IEnumerable<object>> LoadAllAsync(Type type, ResourceFolder folder, string file)
		{
			return LoadAllAsync(type, GetModdingPath(folder, file));
		}

		public static async UniTask<IEnumerable<T>> LoadAllAsync<T>(string path)
		{
			return (await UniTask.WhenAll(ActiveMods.Select(mod => mod.LoadExAsync(typeof(T), path))))
			      .Where(b => b.reference != null)
				  .Select(b => (T) b.reference);
		}

		public static async UniTask<IEnumerable<object>> LoadAllAsync(Type type, string path)
		{
			return (await UniTask.WhenAll(ActiveMods.Select(mod => mod.LoadExAsync(type, path))))
				  .Where(b => b.reference != null)
				  .Select(b => b.reference);
		}

		public static ResourceInfo GetResourceInfo(Type type, ResourceFolder folder, string file)
		{
			return cachedResources.GetOrDefault((type, folder, file));
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
			return ActiveMods.Select(mod => mod.ReadText(path)).FirstOrDefault(text => text != null);
		}

		public static UniTask<string> ReadTextAsync(ResourceFolder folder, string file)
		{
			return ReadTextAsync(GetModdingPath(folder, file));
		}

		public static async UniTask<string> ReadTextAsync(string path)
		{
			foreach (Mod mod in ActiveMods)
			{
				string text = await mod.ReadTextAsync(path);
				if (text != null)
					return text;
			}
			return null;
		}

		public static IEnumerable<string> ReadTextAll(ResourceFolder folder, string file)
		{
			return ReadTextAll(GetModdingPath(folder, file));
		}

		public static IEnumerable<string> ReadTextAll(string path)
		{
			return ActiveMods.Select(mod => mod.ReadText(path)).Where(b => b != null);
		}

		public static UniTask<IEnumerable<string>> ReadTextAllAsync(ResourceFolder folder, string file)
		{
			return ReadTextAllAsync(GetModdingPath(folder, file));
		}

		public static async UniTask<IEnumerable<string>> ReadTextAllAsync(string path)
		{
			return (await UniTask.WhenAll(ActiveMods.Select(mod => mod.ReadTextAsync(path)))).Where(b => b != null);
		}

		public static byte[] ReadBytes(ResourceFolder folder, string file)
		{
			return ReadBytes(GetModdingPath(folder, file));
		}

		public static byte[] ReadBytes(string path)
		{
			return ActiveMods.Select(mod => mod.ReadBytes(path)).FirstOrDefault(bytes => bytes != null);
		}

		public static UniTask<byte[]> ReadBytesAsync(ResourceFolder folder, string file)
		{
			return ReadBytesAsync(GetModdingPath(folder, file));
		}

		public static async UniTask<byte[]> ReadBytesAsync(string path)
		{
			foreach (Mod mod in ActiveMods)
			{
				byte[] bytes = await mod.ReadBytesAsync(path);
				if (bytes != null)
					return bytes;
			}
			return null;
		}

		public static IEnumerable<byte[]> ReadBytesAll(ResourceFolder folder, string file)
		{
			return ReadBytesAll(GetModdingPath(folder, file));
		}

		public static IEnumerable<byte[]> ReadBytesAll(string path)
		{
			return ActiveMods.Select(mod => mod.ReadBytes(path)).Where(b => b != null);
		}

		public static UniTask<IEnumerable<byte[]>> ReadBytesAllAsync(ResourceFolder folder, string file)
		{
			return ReadBytesAllAsync(GetModdingPath(folder, file));
		}

		public static async UniTask<IEnumerable<byte[]>> ReadBytesAllAsync(string path)
		{
			return (await UniTask.WhenAll(ActiveMods.Select(mod => mod.ReadBytesAsync(path)))).Where(b => b != null);
		}

		#endregion

		#region Unloading
		public static void UnloadMods(bool withResources = false)
		{
			int activeCount = ActiveMods.Count;
			int totalCount = 0;
			foreach (ModGroup group in Groups.Values)
				for (int i = group.Mods.Count - 1; i >= 0; i--)
				{
					UnloadModInternal(group.Mods[i], withResources);
					totalCount++;
				}
			ActiveMods = new List<Mod>();

			if (totalCount > 0)
				Debugger.Log("ModManager", $"{totalCount} mods unloaded, {activeCount} of them active.");
		}

		public static void UnloadMod(Mod mod, bool withResources = true)
		{
			UnloadModInternal(mod, withResources);
			RefreshActiveMods();
		}

		private static void UnloadModInternal(Mod mod, bool withResources)
		{
			if (withResources)
				UnloadAll(mod);
			mod.Unload();
			mod.Group.Mods.Remove(mod);
			ModUnloaded?.Invoke(mod);
			Debugger.Log("ModManager", $"Unloaded mod \"{mod.Metadata.Name}\"");
		}

		public static bool UnloadAll(Mod mod)
		{
			bool found = false;
			foreach ((var key, ResourceInfo resource) in cachedResources.Reverse())
				if (resource.Mod == mod)
				{
					UnloadInternal(resource.Reference.Target);
					cachedResources.Remove(key);
					found = true;
					ResourceUnloaded?.Invoke(key.folder, key.file, resource.Mod);
				}

			return found;
		}

		public static bool Unload(object reference)
		{
			(var key, ResourceInfo resource) = cachedResources.FirstOrDefault(kvp => kvp.Value.Reference.Target == reference);

			// Because of FirstOrDefault, kvp.file will be null if key is not found
			if (key.file == null)
				return false;

			UnloadInternal(resource.Reference.Target);
			cachedResources.Remove(key);
			ResourceUnloaded?.Invoke(key.folder, key.file, resource.Mod);

			return true;
		}

		public static bool Unload(Type type, ResourceFolder folder, string file)
		{
			var key = (type, folder, file);

			if (!cachedResources.TryGetValue(key, out ResourceInfo resource))
				return false;

			UnloadInternal(resource.Reference.Target);
			cachedResources.Remove(key);
			ResourceUnloaded?.Invoke(folder, file, resource.Mod);

			return true;
		}

		private static void UnloadInternal(object resource)
		{
			if (resource is Object unityObject)
				unityObject.Destroy();
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
			return Groups.TryGetValue(name, out ModGroup group) ? group.Mods : null;
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
	}
}
#endif