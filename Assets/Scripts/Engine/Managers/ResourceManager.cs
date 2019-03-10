using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UniRx.Async;
using Modding;
using Modding.Parsers;
using Object = UnityEngine.Object;

// Notes:	You have to provide file extension for ResourceFolder other than Resources if it is not loaded
//			by ModManager because you can't enumerate and match files in Data/Resources/StreamingAssets on
//			platforms like Android. If the file is loaded by ModManager it can be loaded without providing
//			an extension since mods are always in an accessible folder which we can enumerate.

public static class ResourceManager
{
	public static readonly Dictionary<ResourceFolder, string> Paths = new Dictionary<ResourceFolder, string>
	{   { ResourceFolder.Data, Application.dataPath + "/"},
		{ ResourceFolder.StreamingAssets, Application.streamingAssetsPath + "/"},
		{ ResourceFolder.PersistentData, Application.persistentDataPath + "/"},
		{ ResourceFolder.Resources, Application.dataPath + "/Resources/"} };

	public static event Action<ResourceFolder, string, object, bool> ResourceLoaded;
	public static event Action<ResourceFolder, string> ResourceUnloaded;

	// Default mode for modding in individual calls
	private const bool DefaultModding = true;

	private static Dictionary<(Type type, ResourceFolder folder, string file), WeakReference> cachedResources
		= new Dictionary<(Type, ResourceFolder, string), WeakReference>();

	#region Loading
	public static T Load<T>(ResourceFolder folder, string file, bool modded = DefaultModding, bool merge = false)
	{
		return (T) Load(typeof(T), folder, file, modded, merge);
	}

	public static object Load(Type type, ResourceFolder folder, string file, bool modded = DefaultModding, bool merge = false)
	{
#if MODDING
		if (modded)
		{
			if (merge)
				return LoadMerged(type, folder, file);

			object moddedFile = ModManager.Load(type, folder, file);
			if (moddedFile != null)
				return moddedFile;
		}
#endif
		return LoadUnmodded(type, folder, file);
	}

	public static async UniTask<T> LoadAsync<T>(ResourceFolder folder, string file, bool modded = DefaultModding, bool merge = false)
	{
		return (T) await LoadAsync(typeof(T), folder, file, modded, merge);
	}

	public static async UniTask<object> LoadAsync(Type type, ResourceFolder folder, string file, bool modded = DefaultModding, bool merge = false)
	{
#if MODDING
		if (modded)
		{
			if (merge)
				return await LoadMergedAsync(type, folder, file);

			object moddedFile = await ModManager.LoadAsync(type, folder, file);
			if (moddedFile != null)
				return moddedFile;
		}
#endif
		return await LoadUnmoddedAsync(type, folder, file);
	}

	private static object LoadCached(Type type, ResourceFolder folder, string file)
	{
		if (!cachedResources.TryGetValue((type, folder, file), out WeakReference weakReference))
			return null;

		object reference = weakReference.Target;
		if (reference == null)
			return null;

		ResourceLoaded?.Invoke(folder, file, reference, false);
		return reference;
	}

	public static T LoadUnmodded<T>(ResourceFolder folder, string file)
	{
		return (T) LoadUnmodded(typeof(T), folder, file);
	}

	public static object LoadUnmodded(Type type, ResourceFolder folder, string file)
	{
		object reference = LoadCached(type, folder, file);
		if (reference != null)
			return reference;

		if (folder == ResourceFolder.Resources)
		{
			string fileNoExt = Path.ChangeExtension(file, null);
			reference = Resources.Load(fileNoExt, type);
		}
		else
		{
			string fullPath = GetPath(folder, file);
			reference = Load(type, fullPath);
		}

		if (reference == null)
			return null;

		// Important to use [key], not Add(key) because the latter generates an error if key exists
		cachedResources[(type, folder, file)] = new WeakReference(reference);
		ResourceLoaded?.Invoke(folder, file, reference, true);
		return reference;
	}

	public static async UniTask<T> LoadUnmoddedAsync<T>(ResourceFolder folder, string file)
	{
		return (T) await LoadUnmoddedAsync(typeof(T), folder, file);
	}

	public static async UniTask<object> LoadUnmoddedAsync(Type type, ResourceFolder folder, string file)
	{
		object reference = LoadCached(type, folder, file);
		if (reference != null)
			return reference;

		if (folder == ResourceFolder.Resources)
		{
			string fileNoExt = Path.ChangeExtension(file, null);
			reference = await Resources.LoadAsync(fileNoExt, type);
		}
		else
		{
			string fullPath = GetPath(folder, file);
			reference = await LoadAsync(type, fullPath);
		}

		if (reference == null)
			return null;

		cachedResources[(type, folder, file)] = new WeakReference(reference);
		ResourceLoaded?.Invoke(folder, file, reference, true);
		return reference;
	}

#if MODDING
	public static T LoadMerged<T>(ResourceFolder folder, string file)
	{
		return (T) LoadMerged(typeof(T), folder, file);
	}

	public static object LoadMerged(Type type, ResourceFolder folder, string file)
	{
		object reference = LoadCached(type, folder, file);
		if (reference != null)
			return reference;

		ResourceParser parser;
		string fullPath = GetPath(folder, file);
		if (folder == ResourceFolder.Resources)
		{
			string fileNoExt = Path.ChangeExtension(file, null);
			reference = Resources.Load(fileNoExt, type);
			if (reference == null)
				return null;
			parser = RankParsers(type, fullPath).FirstOrDefault().parser;
		}
		else
		{
			(reference, parser) = LoadEx(type, fullPath);
			if (reference == null)
				return null;
		}

		object merged = reference;
		if (parser != null)
		{
			try
			{
				if (parser.OperateWith == OperateType.Text)
				{
					List<string> textList = ModManager.ReadTextAll(folder, file);
					foreach (string text in textList)
						parser.Merge(merged, text);
				}
				else
				{
					List<byte[]> bytesList = ModManager.ReadBytesAll(folder, file);
					foreach (byte[] bytes in bytesList)
						parser.Merge(merged, bytes);
				}
			}
			catch (Exception e)
			{
				Debugger.Log("ResourceManager", e.Message);
			}
		}
		cachedResources[(type, folder, file)] = new WeakReference(merged);
		ResourceLoaded?.Invoke(folder, file, merged, true);
		return merged;
	}

	public static async UniTask<T> LoadMergedAsync<T>(ResourceFolder folder, string file)
	{
		return (T) await LoadMergedAsync(typeof(T), folder, file);
	}

	public static async UniTask<object> LoadMergedAsync(Type type, ResourceFolder folder, string file)
	{
		object reference = LoadCached(type, folder, file);
		if (reference != null)
			return reference;

		ResourceParser parser;
		string fullPath = GetPath(folder, file);
		if (folder == ResourceFolder.Resources)
		{
			string fileNoExt = Path.ChangeExtension(file, null);
			reference = (await Resources.LoadAsync(fileNoExt, type));
			if (reference == null)
				return null;
			parser = RankParsers(type, fullPath).FirstOrDefault().parser;
		}
		else
		{
			(reference, parser) = await LoadExAsync(type, fullPath);
			if (reference == null)
				return null;
		}

		object merged = reference;
		if (parser != null)
		{
			try
			{
				if (parser.OperateWith == OperateType.Text)
				{
					List<string> textList = await ModManager.ReadTextAllAsync(folder, file);
					foreach (string text in textList)
						parser.Merge(merged, text);
				}
				else
				{
					List<byte[]> bytesList = await ModManager.ReadBytesAllAsync(folder, file);
					foreach (byte[] bytes in bytesList)
						parser.Merge(merged, bytes);
				}
			}
			catch (Exception e)
			{
				Debugger.Log("ResourceManager", e.Message);
			}
		}
		cachedResources[(type, folder, file)] = new WeakReference(merged);
		ResourceLoaded?.Invoke(folder, file, merged, true);
		return merged;
	}
#endif

	public static T Load<T>(string fullPath)
	{
		return (T) LoadEx(typeof(T), fullPath).reference;
	}

	public static object Load(Type type, string fullPath)
	{
		return LoadEx(type, fullPath).reference;
	}

	public static (T reference, ResourceParser parser) LoadEx<T>(string fullPath)
	{
		var (reference, parser) = LoadEx(typeof(T), fullPath);
		return ((T) reference, parser);
	}

	public static (object reference, ResourceParser parser) LoadEx(Type type, string fullPath)
	{
		string text = null;
		byte[] bytes = null;
		foreach (var (parser, _) in RankParsers(type, fullPath))
		{
			try
			{
				if (parser.OperateWith == OperateType.Text)
				{
					if (text == null)
						text = ReadText(fullPath);
					return text != null ? (parser.Read(type, text, fullPath), parser) : default;
				}
				else
				{
					if (bytes == null)
						bytes = ReadBytes(fullPath);
					return bytes != null ? (parser.Read(type, bytes, fullPath), parser) : default;
				}
			}
			catch (Exception)
			{
			}
		}
		return default;
	}

	public static async UniTask<T> LoadAsync<T>(string fullPath)
	{
		return (T) (await LoadExAsync(typeof(T), fullPath)).reference;
	}

	public static async UniTask<object> LoadAsync(Type type, string fullPath)
	{
		return (await LoadExAsync(type, fullPath)).reference;
	}

	public static async UniTask<(T reference, ResourceParser parser)> LoadExAsync<T>(string fullPath)
	{
		var (reference, parser) = await LoadExAsync(typeof(T), fullPath);
		return ((T) reference, parser);
	}

	public static async UniTask<(object reference, ResourceParser parser)> LoadExAsync(Type type, string fullPath)
	{
		string text = null;
		byte[] bytes = null;
		foreach (var (parser, _) in RankParsers(type, fullPath))
		{
			try
			{
				if (parser.OperateWith == OperateType.Text)
				{
					if (text == null)
						text = await ReadTextAsync(fullPath);
					return text != null ? (parser.Read(type, text, fullPath), parser) : default;
				}
				else
				{
					if (bytes == null)
						bytes = await ReadBytesAsync(fullPath);
					return bytes != null ? (parser.Read(type, bytes, fullPath), parser) : default;
				}
			}
			catch (Exception)
			{
			}
		}
		return default;
	}

	public static bool Unload(object reference)
	{
#if MODDING
		if (ModManager.Unload(reference))
			return true;
#endif

		var key = cachedResources.FirstOrDefault(kvp => kvp.Value.Target == reference).Key;

		if (reference is Object unityObject)
		{
			if (key.file == null || key.folder == ResourceFolder.Resources)
				Resources.UnloadAsset(unityObject);
			else
				Object.Destroy(unityObject);
		}

		// Because of FirstOrDefault, if key is not found "file" will be null
		if (key.file != null)
		{
			cachedResources.Remove(key);
			ResourceUnloaded?.Invoke(key.folder, key.file);
			return true;
		}
		return false;
	}

	public static bool Unload<T>(ResourceFolder folder, string file)
	{
		return Unload(typeof(T), folder, file);
	}

	public static bool Unload(Type type, ResourceFolder folder, string file)
	{

#if MODDING
		if (ModManager.Unload(type, folder, file))
			return true;
#endif

		var key = (type, folder, file);
		if (!cachedResources.TryGetValue(key, out WeakReference weakReference))
			return false;

		if (weakReference.Target is Object unityObject)
		{
			if (folder == ResourceFolder.Resources)
				Resources.UnloadAsset(unityObject);
			else
				Object.Destroy(unityObject);
		}

		cachedResources.Remove(key);
		ResourceUnloaded?.Invoke(folder, file);
		return true;
	}

	public static void UnloadUnused()
	{
		Resources.UnloadUnusedAssets();
	}

	public static void ClearCache()
	{
		cachedResources.Clear();
	}

#endregion

	#region Reading
	public static string ReadText(ResourceFolder folder, string file, bool modded = DefaultModding)
	{
#if MODDING
		if (modded)
		{
			string moddedFile = ModManager.ReadText(folder, file);
			if (moddedFile != null)
				return moddedFile;
		}
#endif
		return ReadText(GetPath(folder, file));
	}

	public static async UniTask<string> ReadTextAsync(ResourceFolder folder, string file, bool modded = DefaultModding)
	{
#if MODDING
		if (modded)
		{
			string moddedFile = await ModManager.ReadTextAsync(folder, file);
			if (moddedFile != null)
				return moddedFile;
		}
#endif
		return await ReadTextAsync(GetPath(folder, file));
	}

	public static byte[] ReadBytes(ResourceFolder folder, string file, bool modded = DefaultModding)
	{
#if MODDING
		if (modded)
		{
			byte[] moddedFile = ModManager.ReadBytes(folder, file);
			if (moddedFile != null)
				return moddedFile;
		}
#endif
		return ReadBytes(GetPath(folder, file));
	}

	public static async UniTask<byte[]> ReadBytesAsync(ResourceFolder folder, string file, bool modded = DefaultModding)
	{
#if MODDING
		if (modded)
		{
			byte[] moddedFile = await ModManager.ReadBytesAsync(folder, file);
			if (moddedFile != null)
				return moddedFile;
		}
#endif
		return await ReadBytesAsync(GetPath(folder, file));
	}

	public static string ReadText(string fullPath)
	{
		try
		{
			return File.ReadAllText(fullPath);
		}
		catch (Exception e)
		{
			Debugger.Log("ResourceManager", e.Message);
			return null;
		}
	}

	public static async UniTask<string> ReadTextAsync(string fullPath)
	{
		UnityWebRequest request = await WebAsync(fullPath);
		if (request.isHttpError || request.isNetworkError)
			return null;
		return request.downloadHandler.text;
	}

	public static byte[] ReadBytes(string fullPath)
	{
		try
		{
			return File.ReadAllBytes(fullPath);
		}
		catch (Exception e)
		{
			Debugger.Log("ResourceManager", e.Message);
			return null;
		}
	}

	public static async UniTask<byte[]> ReadBytesAsync(string fullPath)
	{
		UnityWebRequest request = await WebAsync(fullPath);
		if (request.isHttpError || request.isNetworkError)
			return null;
		return request.downloadHandler.data;
	}

	private static UnityWebRequestAsyncOperation WebAsync(string filePath)
	{
		UnityWebRequest request = UnityWebRequest.Get(LocalToURLPath(filePath));
		return request.SendWebRequest();
	}
	#endregion

	#region Saving/Deleting
	public static bool Save(ResourceFolder folder, string file, object contents)
	{
		return Save(GetPath(folder, file), contents);
	}

	public static UniTask<bool> SaveAsync(ResourceFolder folder, string file, object contents)
	{
		return SaveAsync(GetPath(folder, file), contents);
	}

	public static bool Save(string fullPath, object contents)
	{
		foreach (var (parser, _) in RankParsers(contents.GetType(), fullPath))
		{
			try
			{
				return parser.OperateWith == OperateType.Text ?
						   SaveText(fullPath, (string) parser.Write(contents)) :
						   SaveBytes(fullPath, (byte[]) parser.Write(contents));
			}
			catch (Exception)
			{
			}
		}
		return false;
	}

	public static UniTask<bool> SaveAsync(string fullPath, object contents)
	{
		foreach (var (parser, _) in RankParsers(contents.GetType(), fullPath))
		{
			try
			{
				return parser.OperateWith == OperateType.Text ?
						   SaveTextAsync(fullPath, (string) parser.Write(contents)) :
						   SaveBytesAsync(fullPath, (byte[]) parser.Write(contents));
			}
			catch (Exception)
			{
			}
		}
		return UniTask.FromResult(false);
	}

	public static bool SaveText(ResourceFolder folder, string file, string contents)
	{
		return SaveText(GetPath(folder, file), contents);
	}

	public static UniTask<bool> SaveTextAsync(ResourceFolder folder, string file, string contents)
	{
		return SaveTextAsync(GetPath(folder, file), contents);
	}

	public static bool SaveBytes(ResourceFolder folder, string file, byte[] bytes)
	{
		return SaveBytes(GetPath(folder, file), bytes);
	}

	public static UniTask<bool> SaveBytesAsync(ResourceFolder folder, string file, byte[] bytes)
	{
		return SaveBytesAsync(GetPath(folder, file), bytes);
	}

	public static bool SaveText(string fullPath, string contents)
	{
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
			File.WriteAllText(fullPath, contents);
			return true;
		}
		catch (Exception e)
		{
			Debugger.Log("ResourceManager", e.Message);
			return false;
		}
	}

	public static async UniTask<bool> SaveTextAsync(string fullPath, string contents)
	{
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
			using (StreamWriter stream = new StreamWriter(fullPath))
				await stream.WriteAsync(contents);
			return true;
		}
		catch (Exception e)
		{
			Debugger.Log("ResourceManager", e.Message);
			return false;
		}
	}

	public static bool SaveBytes(string fullPath, byte[] bytes)
	{
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
			File.WriteAllBytes(fullPath, bytes);
			return true;
		}
		catch (Exception e)
		{
			Debugger.Log("ResourceManager", e.Message);
			return false;
		}
	}

	public static async UniTask<bool> SaveBytesAsync(string fullPath, byte[] bytes)
	{
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
			using (FileStream stream = new FileStream(fullPath, FileMode.Create))
				await stream.WriteAsync(bytes, 0, bytes.Length);
			return true;
		}
		catch (Exception e)
		{
			Debugger.Log("ResourceManager", e.Message);
			return false;
		}
	}

	public static bool Delete(ResourceFolder folder, string file)
	{
		return Delete(GetPath(folder, file));
	}

	public static bool Delete(string fullPath)
	{
		try
		{
			File.Delete(fullPath);
			return true;
		}
		catch (Exception e)
		{
			Debugger.Log("ResourceManager", e.Message);
			return false;
		}
	}

	public static bool Exists(ResourceFolder folder, string file)
	{
		return Exists(GetPath(folder, file));
	}

	public static bool Exists(string fullPath)
	{
		return File.Exists(fullPath);
	}
	#endregion

	#region Other
	private static IEnumerable<(ResourceParser parser, float certainty)> RankParsers(Type type, string fullPath)
	{
		return ModManager.Parsers.Select(parser => (parser, certainty: parser.CanOperate(type, fullPath)))
			.Where(d => d.certainty > 0)
			.OrderByDescending(d => d.certainty);
	}

	public static string GetPath(ResourceFolder folder)
	{
		return Paths[folder];
	}

	public static string GetPath(ResourceFolder folder, string file)
	{
		return GetPath(folder) + file;
	}

	private static string LocalToURLPath(string path)
	{
		if (!path.Contains("file://"))
			path = "file://" + path;
		return path;
	}
	#endregion
}