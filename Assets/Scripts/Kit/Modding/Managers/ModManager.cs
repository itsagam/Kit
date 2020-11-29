#if MODDING
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Kit.Modding.Loaders;
using Kit.Parsers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kit.Modding
{
	/// <summary>A manager for loading, executing, configuring, unloading mods and loading resources from them.</summary>
	public static class ModManager
	{
		#region Fields

		/// <summary>Category name to use for logging messages.</summary>
		public const string LogCategory = "ModManager";

		/// <summary><see cref="ModGroup" />s by <see cref="ModType" />.</summary>
		public static readonly Dictionary<ModType, ModGroup> Groups = new Dictionary<ModType, ModGroup>();

		/// <summary>All mod loaders registered with the system.</summary>
		public static readonly List<ModLoader> Loaders = new List<ModLoader>
														 {
															 new DirectModLoader(),
															 new ZipModLoader(),
															 new AssetBundleModLoader()
														 };

		/// <summary>List of all loaded and enabled mods.</summary>
		public static IReadOnlyList<Mod> ActiveMods { get; private set; } = new List<Mod>();

		/// <summary>Event fired when a mod is loaded.</summary>
		public static event Action<Mod> ModLoaded;

		/// <summary>Event fired when a mod is unloaded.</summary>
		public static event Action<Mod> ModUnloaded;

		/// <summary>
		///     <para>Event fired whenever a resource is loaded.</para>
		///     <para>
		///         Returns the folder, path, <see cref="ResourceInfo" />, whether the resource was actually loaded or re-used from cache,
		///         respectively.
		///     </para>
		/// </summary>
		public static event Action<ResourceFolder, string, ResourceInfo, bool> ResourceLoaded;

		/// <summary>
		///     <para>Event fired when a resource is unloaded.</para>
		///     <para>Returns the folder, path, and mod it was loaded with, respectively.</para>
		/// </summary>
		public static event Action<ResourceFolder, string, Mod> ResourceUnloaded;

		/// <summary>A cache of all loaded resources.</summary>
		private static Dictionary<(Type type, ResourceFolder folder, string file), ResourceInfo> cachedResources =
			new Dictionary<(Type, ResourceFolder, string), ResourceInfo>();

		/// <summary>A cache of folder paths.</summary>
		private static Dictionary<ResourceFolder, string> folderToString = new Dictionary<ResourceFolder, string>();

		#endregion

		#region Initialization

		static ModManager()
		{
			AddDefaultGroups();
			CacheFolderNames();

			if (ResourceManager.LogEvents)
			{
				ResourceLoaded += (folder, file, info, loaded) =>
									  Debugger.Log(LogCategory, $"{(loaded ? "Loaded" : "Reused")} \"{file}\" from {folder}.");
				ResourceUnloaded += (folder, file, mod) =>
										Debugger.Log(LogCategory, $"Unloaded \"{file}\" from {folder}.");
			}

			ControlHelper.ApplicationQuit += () => UnloadMods();
		}

		/// <summary>Adds default mod groups.</summary>
		private static void AddDefaultGroups()
		{
			// The order in which groups are added is taken into account in mod order and cannot be changed by any means.
			string accessibleFolder = GetAccessibleFolder();
			AddGroup(new ModGroup(ModType.Patch, accessibleFolder + "Patches/", false, false));
			AddGroup(new ModGroup(ModType.Mod,   accessibleFolder + "Mods/",    true,  true));
		}

		/// <summary>Returns a base folder where mods can be stored.</summary>
		private static string GetAccessibleFolder()
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

		// The "+" operator and Path.Combine are really costly and have a huge performance impact, thus this.
		private static void CacheFolderNames()
		{
			foreach (ResourceFolder value in Enum.GetValues(typeof(ResourceFolder)))
				folderToString[value] = Enum.GetName(typeof(ResourceFolder), value) + "/";
		}

		#endregion

		#region Mod-loading

		/// <summary>Returns potential mod locations by their group.</summary>
		public static Dictionary<ModGroup, string[]> GetModPathsByGroup()
		{
			return Groups.Values
						 .Where(g => Directory.Exists(g.Path))
						 .ToDictionary(g => g, g => Directory.GetFileSystemEntries(g.Path));
		}

		/// <summary>Load the mods.</summary>
		/// <param name="executeScripts">Whether to execute all scripts inside the mod as well.</param>
		public static void LoadMods(bool executeScripts = false)
		{
			LoadMods(GetModPathsByGroup(), executeScripts);
		}

		/// <inheritdoc cref="LoadMods(bool)" />
		/// <param name="modPaths">A dictionary of group and potential mod locations.</param>
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

			Debugger.Log(LogCategory, $"{Mods.Count()} mods loaded, {ActiveMods.Count} active.");

			if (executeScripts)
				ExecuteScripts();
		}

		/// <inheritdoc cref="LoadMods(bool)" />
		/// <summary>Load the mods asynchronously.</summary>
		public static UniTask LoadModsAsync(bool executeScripts = false)
		{
			return LoadModsAsync(GetModPathsByGroup(), executeScripts);
		}

		/// <inheritdoc cref="LoadMods(Dictionary{ModGroup, string[]}, bool)" />
		/// <summary>Load the mods asynchronously.</summary>
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
							Debugger.Log(LogCategory, $"Loaded mod \"{mod.Name}\".");
							break;
						}
					}

			LoadModOrder();
			SaveModOrder();
			RefreshActiveMods();

			Debugger.Log(LogCategory, $"{Mods.Count()} mods loaded, {ActiveMods.Count} active.");

			if (executeScripts)
				await ExecuteScriptsAsync();
		}

		/// <summary>Execute all mod scripts.</summary>
		public static void ExecuteScripts()
		{
			foreach (Mod mod in ActiveMods)
				mod.ExecuteScripts();
		}

		/// <summary>Execute all mod scripts asynchronously.</summary>
		public static async UniTask ExecuteScriptsAsync()
		{
			foreach (Mod mod in ActiveMods)
				await mod.ExecuteScriptsAsync();
		}

		/// <summary>Refreshes the list of active mods.</summary>
		private static void RefreshActiveMods()
		{
			ActiveMods = Groups.SelectMany(kvp => kvp.Value.Mods).Where(IsModEnabled).ToList();
		}

		#endregion

		#region Settings

		/// <summary>Enable a mod.</summary>
		/// <param name="mod">The mod to enable.</param>
		public static void EnableMod(Mod mod)
		{
			ToggleMod(mod, true);
		}

		/// <summary>Disable a mod.</summary>
		/// <param name="mod">The mod to disable.</param>
		public static void DisableMod(Mod mod)
		{
			ToggleMod(mod, false);
		}

		/// <summary>Toggle a mod.</summary>
		/// <param name="mod">The mod to toggle.</param>
		public static void ToggleMod(Mod mod)
		{
			ToggleMod(mod, !IsModEnabled(mod));
		}

		/// <summary>Enable or disable a mod.</summary>
		/// <param name="mod">The mod to enable or disable.</param>
		/// <param name="value">Whether to enable or disable.</param>
		public static void ToggleMod(Mod mod, bool value)
		{
			if (!mod.Group.Deactivatable)
				return;

			SettingsManager.Set(mod.Group.Name.ToString(), mod.Name, "Enabled", value);
			RefreshActiveMods();
		}

		/// <summary>Returns whether a mod is enabled.</summary>
		/// <param name="mod">The mod to check.</param>
		public static bool IsModEnabled(Mod mod)
		{
			if (!mod.Group.Deactivatable)
				return true;

			return SettingsManager.Get(mod.Group.Name.ToString(), mod.Name, "Enabled", true);
		}

		/// <summary>Returns the load order of a mod.</summary>
		/// <param name="mod">The mod to check.</param>
		public static int GetModOrder(Mod mod)
		{
			return ActiveMods.IndexOf(mod);
		}

		/// <summary>Returns the load order of a mod in a group.</summary>
		/// <param name="mod">The mod to check.</param>
		public static int GetModOrderInGroup(Mod mod)
		{
			return mod.Group.Mods.FindIndex(p => p == mod);
		}

		/// <summary>Move a mod to the top.</summary>
		/// <param name="mod">The mod to move.</param>
		public static void MoveModTop(Mod mod)
		{
			MoveModOrder(mod, 0);
		}

		/// <summary>Move a mod to the bottom.</summary>
		/// <param name="mod">The mod to move.</param>
		public static void MoveModBottom(Mod mod)
		{
			MoveModOrder(mod, mod.Group.Mods.Count - 1);
		}

		/// <summary>Move a mod up.</summary>
		/// <param name="mod">The mod to move.</param>
		public static void MoveModUp(Mod mod)
		{
			MoveModOrder(mod, GetModOrderInGroup(mod) - 1);
		}

		/// <summary>Move a mod down.</summary>
		/// <param name="mod">The mod to move.</param>
		public static void MoveModDown(Mod mod)
		{
			MoveModOrder(mod, GetModOrderInGroup(mod) + 1);
		}

		/// <summary>Move a mod to a particular index.</summary>
		/// <param name="mod">The mod to move.</param>
		/// <param name="index">Index to move to.</param>
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

		/// <summary>Load mod references according to their order from settings.</summary>
		private static void LoadModOrder()
		{
			// Reversing the list makes sure new mods (whose entries we do not have and will have all the same value -1)
			// are ordered in reverse (newer on top)
			foreach (ModGroup group in Groups.Values)
				if (group.Reorderable)
					group.Mods = group.Mods.AsEnumerable()
									  .Reverse()
									  .OrderBy(mod => SettingsManager.Get(group.Name.ToString(),
																		  mod.Name,
																		  "Order",
																		  -1))
									  .ToList();
		}

		/// <summary>Save mod order in settings according to references.</summary>
		private static void SaveModOrder()
		{
			foreach (ModGroup group in Groups.Values)
				if (group.Reorderable)
					for (int order = 0; order < group.Mods.Count; order++)
					{
						Mod mod = group.Mods[order];
						SettingsManager.Set(group.Name.ToString(), mod.Name, "Order", order);
					}
		}

		#endregion

		#region Resource-loading

		/// <summary>Load a resource from cache.</summary>
		/// <param name="type">Type of the resource expected.</param>
		/// <param name="folder">The folder to load the resource from.</param>
		/// <param name="file">The path and file-name relative to the <paramref name="folder" />.</param>
		/// <returns>Reference to the resource, or <see langword="null" /> if it was not found.</returns>
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

		/// <summary>Load a resource.</summary>
		/// <typeparam name="T">Type of the resource expected.</typeparam>
		/// <param name="folder">The folder to load the resource from.</param>
		/// <param name="file">The path and file-name relative to the <paramref name="folder" />.</param>
		/// <returns>Reference to the resource, or <see langword="null" /> if it was not found.</returns>
		public static object Load<T>(ResourceFolder folder, string file)
		{
			return (T) Load(typeof(T), folder, file);
		}

		/// <inheritdoc cref="Load{T}(ResourceFolder, string)" />
		/// <param name="type">Type of the resource expected.</param>
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
				(object reference, string filePath, ResourceParser parser) = mod.LoadEx(type, path);
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

		/// <inheritdoc cref="Load{T}(ResourceFolder, string)" />
		/// <summary>Load a resource asynchronously.</summary>
		public static async UniTask<T> LoadAsync<T>(ResourceFolder folder, string file)
		{
			return (T) await LoadAsync(typeof(T), folder, file);
		}

		/// <inheritdoc cref="Load(Type, ResourceFolder, string)" />
		/// <summary>Load a resource asynchronously.</summary>
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
				(object reference, string filePath, ResourceParser parser) = await mod.LoadExAsync(type, path);
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

		/// <inheritdoc cref="Load{T}(ResourceFolder, string)" />
		/// <summary>Load a resource from all mods that have it.</summary>
		public static IEnumerable<T> LoadAll<T>(ResourceFolder folder, string file)
		{
			return LoadAll<T>(GetModdingPath(folder, file));
		}

		/// <inheritdoc cref="LoadAll{T}(ResourceFolder, string)" />
		/// <param name="type">Type of the resource expected.</param>
		public static IEnumerable<object> LoadAll(Type type, ResourceFolder folder, string file)
		{
			return LoadAll(type, GetModdingPath(folder, file));
		}

		/// <inheritdoc cref="LoadAll{T}(ResourceFolder, string)" />
		/// <param name="path">Path to the resource.</param>
		public static IEnumerable<T> LoadAll<T>(string path)
		{
			return ActiveMods.Select(mod => (T) mod.LoadEx(typeof(T), path).reference).Where(b => b != null);
		}

		/// <inheritdoc cref="LoadAll(Type, ResourceFolder, string)" />
		/// <param name="path">Path to the resource.</param>
		public static IEnumerable<object> LoadAll(Type type, string path)
		{
			return ActiveMods.Select(mod => mod.LoadEx(type, path).reference).Where(b => b != null);
		}

		/// <inheritdoc cref="LoadAll{T}(ResourceFolder, string)" />
		/// <summary>Load a resource from all mods that have it asynchronously.</summary>
		public static UniTask<IEnumerable<T>> LoadAllAsync<T>(ResourceFolder folder, string file)
		{
			return LoadAllAsync<T>(GetModdingPath(folder, file));
		}

		/// <inheritdoc cref="LoadAll(Type, ResourceFolder, string)" />
		/// <summary>Load a resource from all mods that have it asynchronously.</summary>
		public static UniTask<IEnumerable<object>> LoadAllAsync(Type type, ResourceFolder folder, string file)
		{
			return LoadAllAsync(type, GetModdingPath(folder, file));
		}

		/// <inheritdoc cref="LoadAll{T}(string)" />
		/// <summary>Load a resource from all mods that have it asynchronously.</summary>
		public static async UniTask<IEnumerable<T>> LoadAllAsync<T>(string path)
		{
			return (await UniTask.WhenAll(ActiveMods.Select(mod => mod.LoadExAsync(typeof(T), path))))
				  .Where(b => b.reference != null)
				  .Select(b => (T) b.reference);
		}

		/// <inheritdoc cref="LoadAll(Type, string)" />
		/// <summary>Load a resource from all mods that have it asynchronously.</summary>
		public static async UniTask<IEnumerable<object>> LoadAllAsync(Type type, string path)
		{
			return (await UniTask.WhenAll(ActiveMods.Select(mod => mod.LoadExAsync(type, path))))
				  .Where(b => b.reference != null)
				  .Select(b => b.reference);
		}

		/// <summary>Returns the information about a cached resource.</summary>
		/// <param name="type">Type of the resource expected.</param>
		/// <param name="folder">The folder to load the resource from.</param>
		/// <param name="file">The path and file-name relative to the <paramref name="folder" />.</param>
		public static ResourceInfo GetResourceInfo(Type type, ResourceFolder folder, string file)
		{
			return cachedResources.GetOrDefault((type, folder, file));
		}

		/// <summary>Returns the information about a cached resource.</summary>
		/// <param name="resource">Reference to the resource.</param>
		public static ResourceInfo GetResourceInfo(object resource)
		{
			return cachedResources.FirstOrDefault(r => r.Value.Reference.Target == resource).Value;
		}

		#endregion

		#region Reading

		/// <inheritdoc cref="ResourceManager.ReadText(ResourceFolder, string, bool)" />
		public static string ReadText(ResourceFolder folder, string file)
		{
			return ReadText(GetModdingPath(folder, file));
		}

		/// <inheritdoc cref="ResourceManager.ReadText(string)" />
		public static string ReadText(string path)
		{
			return ActiveMods.Select(mod => mod.ReadText(path)).FirstOrDefault(text => text != null);
		}

		/// <inheritdoc cref="ResourceManager.ReadTextAsync(ResourceFolder, string, bool)" />
		public static UniTask<string> ReadTextAsync(ResourceFolder folder, string file)
		{
			return ReadTextAsync(GetModdingPath(folder, file));
		}

		/// <inheritdoc cref="ResourceManager.ReadTextAsync(string)" />
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

		/// <summary>Read contents of a file in text-mode from all mods that have it.</summary>
		/// <param name="folder">The folder to read the file from.</param>
		/// <param name="file">The path and file-name relative to the <paramref name="folder" />.</param>
		/// <returns>A list of byte-arrays with contents of the files.</returns>
		public static IEnumerable<string> ReadTextAll(ResourceFolder folder, string file)
		{
			return ReadTextAll(GetModdingPath(folder, file));
		}

		/// <inheritdoc cref="ReadTextAll(ResourceFolder, string)" />
		/// <param name="path">Path to the resource.</param>
		public static IEnumerable<string> ReadTextAll(string path)
		{
			return ActiveMods.Select(mod => mod.ReadText(path)).Where(b => b != null);
		}

		/// <inheritdoc cref="ReadTextAll(ResourceFolder, string)" />
		/// <summary>Read contents of a file in text-mode asynchronously from all mods that have it.</summary>
		public static UniTask<IEnumerable<string>> ReadTextAllAsync(ResourceFolder folder, string file)
		{
			return ReadTextAllAsync(GetModdingPath(folder, file));
		}

		/// <inheritdoc cref="ReadTextAllAsync(ResourceFolder, string)" />
		/// <param name="path">Path to the resource.</param>
		public static async UniTask<IEnumerable<string>> ReadTextAllAsync(string path)
		{
			return (await UniTask.WhenAll(ActiveMods.Select(mod => mod.ReadTextAsync(path)))).Where(b => b != null);
		}

		/// <inheritdoc cref="ResourceManager.ReadBytes(ResourceFolder, string, bool)" />
		public static byte[] ReadBytes(ResourceFolder folder, string file)
		{
			return ReadBytes(GetModdingPath(folder, file));
		}

		/// <inheritdoc cref="ResourceManager.ReadBytes(string)" />
		public static byte[] ReadBytes(string path)
		{
			return ActiveMods.Select(mod => mod.ReadBytes(path)).FirstOrDefault(bytes => bytes != null);
		}

		/// <inheritdoc cref="ResourceManager.ReadBytesAsync(ResourceFolder, string, bool)" />
		public static UniTask<byte[]> ReadBytesAsync(ResourceFolder folder, string file)
		{
			return ReadBytesAsync(GetModdingPath(folder, file));
		}

		/// <inheritdoc cref="ResourceManager.ReadBytesAsync(string)" />
		public static async UniTask<byte[]> ReadBytesAsync(string path)
		{
			foreach (Mod mod in ActiveMods)
			{
				var bytes = await mod.ReadBytesAsync(path);
				if (bytes != null)
					return bytes;
			}

			return null;
		}

		/// <summary>Read contents of a file in binary-mode from all mods that have it.</summary>
		/// <param name="folder">The folder to read the file from.</param>
		/// <param name="file">The path and file-name relative to the <paramref name="folder" />.</param>
		/// <returns>A list of byte-arrays with contents of the files.</returns>
		public static IEnumerable<byte[]> ReadBytesAll(ResourceFolder folder, string file)
		{
			return ReadBytesAll(GetModdingPath(folder, file));
		}

		/// <inheritdoc cref="ReadBytesAll(ResourceFolder, string)" />
		/// <param name="path">Path to the resource.</param>
		public static IEnumerable<byte[]> ReadBytesAll(string path)
		{
			return ActiveMods.Select(mod => mod.ReadBytes(path)).Where(b => b != null);
		}

		/// <inheritdoc cref="ReadBytesAll(ResourceFolder, string)" />
		/// <summary>Read contents of a file in binary-mode asynchronously from all mods that have it.</summary>
		public static UniTask<IEnumerable<byte[]>> ReadBytesAllAsync(ResourceFolder folder, string file)
		{
			return ReadBytesAllAsync(GetModdingPath(folder, file));
		}

		/// <inheritdoc cref="ReadBytesAllAsync(ResourceFolder, string)" />
		/// <param name="path">Path to the resource.</param>
		public static async UniTask<IEnumerable<byte[]>> ReadBytesAllAsync(string path)
		{
			return (await UniTask.WhenAll(ActiveMods.Select(mod => mod.ReadBytesAsync(path)))).Where(b => b != null);
		}

		#endregion

		#region Unloading

		/// <summary>Unload all active mods.</summary>
		/// <param name="withResources">Whether to unload all resources loaded by them as well.</param>
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
				Debugger.Log(LogCategory, $"{totalCount} mods unloaded, {activeCount} of them active.");
		}

		/// <summary>Unload a mod.</summary>
		/// <param name="mod">The mod to unload.</param>
		/// <param name="withResources">Whether to unload all resources loaded by the mod as well.</param>
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
			Debugger.Log(LogCategory, $"Unloaded mod \"{mod.Name}\".");
		}

		/// <summary>Unload all resources loaded by a mod.</summary>
		/// <param name="mod">The mod to unload resources of.</param>
		/// <returns>Whether any resources were found and unloaded.</returns>
		public static bool UnloadAll(Mod mod)
		{
			bool found = false;
			foreach (((Type type, ResourceFolder folder, string file) key, ResourceInfo resource) in cachedResources.Reverse())
				if (resource.Mod == mod)
				{
					UnloadInternal(resource.Reference.Target);
					cachedResources.Remove(key);
					found = true;
					ResourceUnloaded?.Invoke(key.folder, key.file, resource.Mod);
				}

			return found;
		}

		/// <inheritdoc cref="ResourceManager.Unload(object)" />
		public static bool Unload(object reference)
		{
			((Type type, ResourceFolder folder, string file) key, ResourceInfo resource) =
				cachedResources.FirstOrDefault(kvp => kvp.Value.Reference.Target == reference);

			// Because of FirstOrDefault, kvp.file will be null if key is not found
			if (key.file == null)
				return false;

			UnloadInternal(resource.Reference.Target);
			cachedResources.Remove(key);
			ResourceUnloaded?.Invoke(key.folder, key.file, resource.Mod);

			return true;
		}

		/// <inheritdoc cref="ResourceManager.Unload(Type, ResourceFolder, string)" />
		public static bool Unload(Type type, ResourceFolder folder, string file)
		{
			(Type type, ResourceFolder folder, string file) key = (type, folder, file);

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

		/// <summary>Add a mod group.</summary>
		public static void AddGroup(ModGroup group)
		{
			Groups.Add(group.Name, group);
		}

		/// <summary>Remove a mod group.</summary>
		public static void RemoveGroup(ModGroup group)
		{
			Groups.Remove(group.Name);
		}

		/// <summary>Returns all mods in a given group</summary>
		public static List<Mod> GetModsByGroup(ModType name)
		{
			return Groups.TryGetValue(name, out ModGroup group) ? group.Mods : null;
		}

		/// <summary>Returns the relative path to a <see cref="ResourceFolder" />.</summary>
		public static string GetModdingPath(ResourceFolder folder)
		{
			return folderToString[folder];
		}

		/// <summary>Returns the relative path to a file.</summary>
		/// <param name="folder">The folder to check in.</param>
		/// <param name="file">The path and file-name relative to the <paramref name="folder" />.</param>
		/// <returns>The file path combined with folder.</returns>
		public static string GetModdingPath(ResourceFolder folder, string file)
		{
			return GetModdingPath(folder) + file;
		}

		/// <summary>Clear the cache.</summary>
		public static void ClearCache()
		{
			cachedResources.Clear();
		}

		/// <summary>Returns the list of all loaded mods, enabled or disabled.</summary>
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