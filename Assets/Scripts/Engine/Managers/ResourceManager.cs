using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UniRx.Async;
using Modding;
using Modding.Parsers;

// Limitations: 1)	You have to provide file extension for ResourceFolder other than Resources if its not loaded 
//					by ModManager because you can't enumerate and match files in Data/Resources/StreamingAssets on 
//					platforms like Android. If the file is found in ModManager they can be loaded without providing 
//					an extension since mods are always in an accessible folder which we can enumerate.
//				2)	If two files with same name are loaded from ModManager without an extension, it'll create a 
//					conflict (ModManager.Load<AudioClip>("Files/Test") and ModManager.Load<Texture>("Files/Test")),
//					since the first object would be cached and a second call would try to reuse and cast it.
//					TODO: Solve this by making cache in ResourceManager a list (ModManager already is) and comparing
//					types as well as path. If either does not match, find and load a new file otherwise return null.
//				3)	WeakReferences work as intended on System.Object but UnityEngine.Objects are only garbage collected
//					on scene load.

public enum ResourceFolder
{
	Data,
	StreamingAssets,
	PersistentData,
	Resources
}

public class ResourceManager
{
	public static Dictionary<ResourceFolder, string> Paths = new Dictionary<ResourceFolder, string>
	{   { ResourceFolder.Data, Application.dataPath + "/"},
		{ ResourceFolder.StreamingAssets, Application.streamingAssetsPath + "/"},
		{ ResourceFolder.PersistentData, Application.persistentDataPath + "/"},
		{ ResourceFolder.Resources, Application.dataPath + "/Resources/"} };

	// Default mode for modding in individual calls
	public const bool DefaultModding = true;

	public static event Action<ResourceFolder, string, object> ResourceLoaded;
	public static event Action<ResourceFolder, string, object> ResourceReused;

	// TODO: Profile memory and do memory management
	protected static Dictionary<string, WeakReference> resources = new Dictionary<string, WeakReference>();
	protected static Dictionary<ResourceFolder, string> folderToString = new Dictionary<ResourceFolder, string>();

	static ResourceManager()
	{
		foreach (var kvp in Paths)
			folderToString[kvp.Key] = kvp.Key.ToString() + "/";
	}

	#region Loading
	public static T Load<T>(ResourceFolder folder, string file, bool modded = DefaultModding, bool merge = false) where T : class
	{
#if MODDING
		if (modded)
		{
			if (merge)
			{
				return LoadMerged<T>(folder, file);
			}
			else
			{
				string cachePath = GetCachePath(folder, file);
				T moddedFile = ModManager.Load<T>(cachePath);
				if (moddedFile != null)
					return moddedFile;
			}		
		}
#endif
		return LoadUnmodded<T>(folder, file);
	}

	public static async UniTask<T> LoadAsync<T>(ResourceFolder folder, string file, bool modded = DefaultModding, bool merge = false) where T : class
	{
#if MODDING
		if (modded)
		{
			if (merge)
			{
				return await LoadMergedAsync<T>(folder, file);
			}
			else
			{
				string cachePath = GetCachePath(folder, file);
				T moddedFile = await ModManager.LoadAsync<T>(cachePath);
				if (moddedFile != null)
					return moddedFile;
			}
		}
#endif
		return await LoadUnmoddedAsync<T>(folder, file);
	}

	public static T LoadUnmodded<T>(ResourceFolder folder, string file) where T : class
	{
		object reference = null;
		string cachePath = GetCachePath(folder, file);
		if (resources.TryGetValue(cachePath, out WeakReference weakReference) && weakReference.IsAlive)
		{
			reference = weakReference.Target;
			ResourceReused.Invoke(folder, file, reference);
			return (T) reference;
		}

		if (folder == ResourceFolder.Resources)
		{
			string fileNoExt = Path.ChangeExtension(file, null);
			reference = Resources.Load(fileNoExt);
		}
		else
		{
			string fullPath = GetPath(folder, file);
			reference = Load<T>(fullPath).reference;
		}

		if (reference != null)
		{
			resources.Add(cachePath, new WeakReference(reference));
			ResourceLoaded?.Invoke(folder, file, reference);
		}
		return (T) reference;
	}

	public static async UniTask<T> LoadUnmoddedAsync<T>(ResourceFolder folder, string file) where T : class
	{
		object reference = null;
		string cachePath = GetCachePath(folder, file);
		if (resources.TryGetValue(cachePath, out WeakReference weakReference) && weakReference.IsAlive)
		{
			reference = weakReference.Target;
			ResourceReused.Invoke(folder, file, reference);
			return (T) reference;
		}

		if (folder == ResourceFolder.Resources)
		{
			string fileNoExt = Path.ChangeExtension(file, null);
			reference = await Resources.LoadAsync(fileNoExt);
		}
		else
		{
			string fullPath = GetPath(folder, file);
			reference = (await LoadAsync<T>(fullPath)).reference;
		}

		if (reference != null)
		{
			resources.Add(cachePath, new WeakReference(reference));
			ResourceLoaded?.Invoke(folder, file, reference);
		}
		return (T) reference;
	}

#if MODDING
	public static T LoadMerged<T>(ResourceFolder folder, string file) where T : class
	{
		object reference = null;
		string cachePath = GetCachePath(folder, file);
		if (resources.TryGetValue(cachePath, out WeakReference weakReference) && weakReference.IsAlive)
		{
			reference = weakReference.Target;
			ResourceReused.Invoke(folder, file, reference);
			return (T) reference;
		}

		ResourceParser parser = null;
		string fullPath = GetPath(folder, file);
		if (folder == ResourceFolder.Resources)
		{
			string fileNoExt = Path.ChangeExtension(file, null);
			reference = Resources.Load(fileNoExt);
			if (reference == null)
				return null;
			parser = RankParsers<T>(fullPath).FirstOrDefault().parser;
		}
		else
		{
			(reference, parser) = Load<T>(fullPath);
			if (reference == null)
				return null;
		}

		T merged = (T) reference;
		if (parser != null)
		{
			try
			{
				if (parser.OperateWith == OperateType.Text)
				{
					List<string> textList = ModManager.ReadTextAll(cachePath);
					foreach (string text in textList)
						parser.Merge<T>(merged, text);
				}
				else
				{
					List<byte[]> bytesList = ModManager.ReadBytesAll(cachePath);
					foreach (byte[] bytes in bytesList)
						parser.Merge<T>(merged, bytes);
				}
			}
			catch (Exception e)
			{
				Debugger.Log("ResourceManager", e.Message);
			}
		}
		resources.Add(cachePath, new WeakReference(merged));
		ResourceLoaded.Invoke(folder, file, merged);
		return merged;
	}

	public static async UniTask<T> LoadMergedAsync<T>(ResourceFolder folder, string file) where T : class
	{
		object reference = null;
		string cachePath = GetCachePath(folder, file);
		if (resources.TryGetValue(cachePath, out WeakReference weakReference) && weakReference.IsAlive)
		{
			reference = weakReference.Target;
			ResourceReused.Invoke(folder, file, reference);
			return (T) reference;
		}

		ResourceParser parser = null;
		string fullPath = GetPath(folder, file);
		if (folder == ResourceFolder.Resources)
		{
			string fileNoExt = Path.ChangeExtension(file, null);
			reference = await Resources.LoadAsync(fileNoExt);
			if (reference == null)
				return null;
			parser = RankParsers<T>(fullPath).FirstOrDefault().parser;
		}
		else
		{
			(reference, parser) = await LoadAsync<T>(fullPath);
			if (reference == null)
				return null;
		}

		T merged = (T) reference;
		if (parser != null)
		{
			try
			{
				if (parser.OperateWith == OperateType.Text)
				{
					List<string> textList = await ModManager.ReadTextAllAsync(cachePath);
					foreach (string text in textList)
						parser.Merge<T>(merged, text);
				}
				else
				{
					List<byte[]> bytesList = await ModManager.ReadBytesAllAsync(cachePath);
					foreach (byte[] bytes in bytesList)
						parser.Merge<T>(merged, bytes);
				}
			}
			catch (Exception e)
			{
				Debugger.Log("ResourceManager", e.Message);
			}
		}
		resources.Add(cachePath, new WeakReference(merged));
		ResourceLoaded.Invoke(folder, file, merged);
		return merged;
	}
#endif

	public static (T reference, ResourceParser parser) Load<T>(string fullPath) where T : class
	{
		string text = null;
		byte[] bytes = null;
		foreach (var (parser, certainty) in RankParsers<T>(fullPath))
		{
			try
			{
				if (parser.OperateWith == OperateType.Text)
				{
					if (text == null)
						text = ReadText(fullPath);
					return text != null ? ((T) parser.Read<T>(text, fullPath), parser) : default;
				}
				else
				{
					if (bytes == null)
						bytes = ReadBytes(fullPath);
					return bytes != null ? ((T) parser.Read<T>(bytes, fullPath), parser) : default;
				}
			}
			catch (Exception)
			{
			}
		}
		return default;
	}

	public static async UniTask<(T reference, ResourceParser parser)> LoadAsync<T>(string fullPath) where T : class
	{
		string text = null;
		byte[] bytes = null;
		foreach (var (parser, certainty) in RankParsers<T>(fullPath))
		{
			try
			{
				if (parser.OperateWith == OperateType.Text)
				{
					if (text == null)
						text = await ReadTextAsync(fullPath);
					return text != null ? ((T) parser.Read<T>(text, fullPath), parser) : default;
				}
				else
				{
					if (bytes == null)
						bytes = await ReadBytesAsync(fullPath);
					return bytes != null ? ((T) parser.Read<T>(bytes, fullPath), parser) : default;
				}
			}
			catch (Exception)
			{
			}
		}
		return default;
	}

	protected static IEnumerable<(ResourceParser parser, float certainty)> RankParsers<T>(string fullPath)
	{
		return ModManager.Parsers.Select(parser => (parser, certainty: parser.CanRead<T>(fullPath)))
			.Where(d => d.certainty > 0)
			.OrderByDescending(d => d.certainty);
	}

	public static bool Unload(object resource)
	{
#if MODDING
		if (ModManager.Unload(resource))
			return true;
#endif

		string cachePath = null;
		foreach (var kvp in resources.Reverse())
			if (kvp.Value.Target == resource)
			{
				cachePath = kvp.Key;
				resources.Remove(cachePath);
				break;
			}

		if (resource is UnityEngine.Object unityObject)
		{
			try
			{
				// GetInstanceID() seems to return positive values for loaded assets and negative for created ones				
				//if (unityObject.GetInstanceID() > 0)
				if (cachePath == null || cachePath.IsLeft(GetCachePath(ResourceFolder.Resources)))
					Resources.UnloadAsset(unityObject);
				else
					UnityEngine.Object.Destroy(unityObject);
			}
			catch (Exception)
			{
			}
		}

		return cachePath != null;
	}

	public static bool Unload(ResourceFolder folder, string file)
	{
		string cachePath = GetCachePath(folder, file);

#if MODDING
		if (ModManager.UnloadAll(cachePath))
			return true;
#endif
		
		if (resources.TryGetValue(cachePath, out WeakReference weakReference))
		{
			resources.Remove(cachePath);
			if (weakReference.Target is UnityEngine.Object unityObject)
			{
				if (folder == ResourceFolder.Resources)
					Resources.UnloadAsset(unityObject);
				else
					UnityEngine.Object.Destroy(unityObject);
			}
			return true;
		}
		return false;
	}

	public static void UnloadUnused()
	{
		Resources.UnloadUnusedAssets();
	}

	public static void ClearCache()
	{
		resources.Clear();
	}

#endregion

#region Reading
	public static string ReadText(ResourceFolder folder, string file, bool modded = DefaultModding)
	{
#if MODDING
		if (modded)
		{
			string moddedFile = ModManager.ReadText(GetCachePath(folder, file));
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
			string moddedFile = await ModManager.ReadTextAsync(GetCachePath(folder, file));
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
			byte[] moddedFile = ModManager.ReadBytes(GetCachePath(folder, file));
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
			byte[] moddedFile = await ModManager.ReadBytesAsync(GetCachePath(folder, file));
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
		else
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
		else
			return request.downloadHandler.data;
	}

	public static async UniTask<UnityWebRequest> WebAsync(string filePath)
	{
		UnityWebRequest request = UnityWebRequest.Get(LocalToURLPath(filePath));
		await request.SendWebRequest();
		return request;
	}
#endregion

#region Saving/Deleting
	public static bool Save(ResourceFolder folder, string file, object contents, ResourceParser parser)
	{
		return Save(GetPath(folder, file), contents, parser);
	}

	public static async UniTask<bool> SaveAsync(ResourceFolder folder, string file, object contents, ResourceParser parser)
	{
		return await SaveAsync(GetPath(folder, file), contents, parser);
	}

	public static bool Save(string fullPath, object contents, ResourceParser parser)
	{
		if (parser.OperateWith == OperateType.Text)
			return SaveText(fullPath, (string)parser.Write(contents));
		else
			return SaveBytes(fullPath, (byte[])parser.Write(contents));
	}

	public static async UniTask<bool> SaveAsync(string fullPath, object contents, ResourceParser parser)
	{
		if (parser.OperateWith == OperateType.Text)
			return await SaveTextAsync(fullPath, (string)parser.Write(contents));
		else
			return await SaveBytesAsync(fullPath, (byte[])parser.Write(contents));
	}

	public static bool SaveText(ResourceFolder folder, string file, string contents)
	{
		return SaveText(GetPath(folder, file), contents);
	}

	public static async UniTask<bool> SaveTextAsync(ResourceFolder folder, string file, string contents)
	{
		return await SaveTextAsync(GetPath(folder, file), contents);
	}

	public static bool SaveBytes(ResourceFolder folder, string file, byte[] bytes)
	{
		return SaveBytes(GetPath(folder, file), bytes);
	}

	public static async UniTask<bool> SaveBytesAsync(ResourceFolder folder, string file, byte[] bytes)
	{
		return await SaveBytesAsync(GetPath(folder, file), bytes);
	}

	public static bool SaveText(string fullPath, string contents)
	{
		try
		{
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
#if MODDING
	public static List<ResourceInfo> GetModdedResourceInfo(ResourceFolder folder, string file)
	{
		return ModManager.GetResources(GetCachePath(folder, file));
	}

	public static ResourceInfo? GetModdedResourceInfo(object resource)
	{
		return ModManager.GetResourceInfo(resource);
	}
#endif

	public static string GetPath(ResourceFolder folder)
	{
		return Paths[folder];
	}

	public static string GetPath(ResourceFolder folder, string file)
	{
		return GetPath(folder) + file;
	}

	public static string GetCachePath(ResourceFolder folder)
	{
		return folderToString[folder];
	}

	public static string GetCachePath(ResourceFolder folder, string file)
	{
		return folderToString[folder] + file;
	}

	public static string LocalToURLPath(string path)
	{
		if (!path.Contains("file://"))
			path = "file://" + path;
		return path;
	}
#endregion
}