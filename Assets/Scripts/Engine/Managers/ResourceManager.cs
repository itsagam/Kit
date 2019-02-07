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

// Notes:	You have to provide file extension for ResourceFolder other than Resources if it is not loaded
//			by ModManager because you can't enumerate and match files in Data/Resources/StreamingAssets on 
//			platforms like Android. If the file is loaded by ModManager it can be loaded without providing 
//			an extension since mods are always in an accessible folder which we can enumerate.

public class ResourceManager
{
	public static Dictionary<ResourceFolder, string> Paths = new Dictionary<ResourceFolder, string>
	{   { ResourceFolder.Data, Application.dataPath + "/"},
		{ ResourceFolder.StreamingAssets, Application.streamingAssetsPath + "/"},
		{ ResourceFolder.PersistentData, Application.persistentDataPath + "/"},
		{ ResourceFolder.Resources, Application.dataPath + "/Resources/"} };

	// Default mode for modding in individual calls
	public const bool DefaultModding = true;

	public static event Action<ResourceFolder, string, object, bool> ResourceLoaded;
	public static event Action<ResourceFolder, string> ResourceUnloaded;

	protected static Dictionary<(Type type, ResourceFolder folder, string file), WeakReference> cachedResources 
		= new Dictionary<(Type, ResourceFolder, string), WeakReference>();

	#region Loading
	// The "where" clause does three things:
	// 1) Allows to cast from UnityEngine.Object to T with "obj as T"
	// 2) Allows to pass T to Resource.Load, otherwise there's an error
	// 3) Allows to return null
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
				T moddedFile = ModManager.Load<T>(folder, file);
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
				T moddedFile = await ModManager.LoadAsync<T>(folder, file);
				if (moddedFile != null)
					return moddedFile;
			}
		}
#endif
		return await LoadUnmoddedAsync<T>(folder, file);
	}

	protected static T LoadCached<T>(ResourceFolder folder, string file) where T : class
	{
		if (cachedResources.TryGetValue((typeof(T), folder, file), out WeakReference weakReference))
		{
			object reference = weakReference.Target;
			if (reference != null)
			{
				ResourceLoaded?.Invoke(folder, file, reference, false);
				return (T) reference;
			}
		}
		return null;
	}

	public static T LoadUnmodded<T>(ResourceFolder folder, string file) where T : class
	{
		T reference = LoadCached<T>(folder, file);
		if (reference != null)
			return reference;

		Type type = typeof(T);
		if (folder == ResourceFolder.Resources)
		{
			string fileNoExt = Path.ChangeExtension(file, null);
			reference = Resources.Load(fileNoExt, type) as T;
		}
		else
		{
			string fullPath = GetPath(folder, file);
			reference = Load<T>(fullPath).reference;
		}

		if (reference != null)
		{
			// Important to use [key], not Add(key) because the latter generates an error if key exists
			cachedResources[(type, folder, file)] = new WeakReference(reference);
			ResourceLoaded?.Invoke(folder, file, reference, true);
		}
		return reference;
	}

	public static async UniTask<T> LoadUnmoddedAsync<T>(ResourceFolder folder, string file) where T : class
	{
		T reference = LoadCached<T>(folder, file);
		if (reference != null)
			return reference;

		Type type = typeof(T);
		if (folder == ResourceFolder.Resources)
		{
			string fileNoExt = Path.ChangeExtension(file, null);
			reference = (await Resources.LoadAsync(fileNoExt, type)) as T;
		}
		else
		{
			string fullPath = GetPath(folder, file);
			reference = (await LoadAsync<T>(fullPath)).reference;
		}

		if (reference != null)
		{
			cachedResources[(type, folder, file)] = new WeakReference(reference);
			ResourceLoaded?.Invoke(folder, file, reference, true);
		}
		return reference;
	}

#if MODDING
	public static T LoadMerged<T>(ResourceFolder folder, string file) where T : class
	{
		T reference = LoadCached<T>(folder, file);
		if (reference != null)
			return reference;

		ResourceParser parser = null;
		string fullPath = GetPath(folder, file);
		if (folder == ResourceFolder.Resources)
		{
			string fileNoExt = Path.ChangeExtension(file, null);
			reference = Resources.Load(fileNoExt) as T;
			if (reference == null)
				return null;
			parser = RankParsers(fullPath, typeof(T)).FirstOrDefault().parser;
		}
		else
		{
			(reference, parser) = Load<T>(fullPath);
			if (reference == null)
				return null;
		}

		T merged = reference;
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
		cachedResources[(typeof(T), folder, file)] = new WeakReference(merged);
		ResourceLoaded?.Invoke(folder, file, merged, true);
		return merged;
	}

	public static async UniTask<T> LoadMergedAsync<T>(ResourceFolder folder, string file) where T : class
	{
		T reference = LoadCached<T>(folder, file);
		if (reference != null)
			return reference;

		ResourceParser parser = null;
		string fullPath = GetPath(folder, file);
		if (folder == ResourceFolder.Resources)
		{
			string fileNoExt = Path.ChangeExtension(file, null);
			reference = (await Resources.LoadAsync(fileNoExt)) as T;
			if (reference == null)
				return null;
			parser = RankParsers(fullPath, typeof(T)).FirstOrDefault().parser;
		}
		else
		{
			(reference, parser) = await LoadAsync<T>(fullPath);
			if (reference == null)
				return null;
		}

		T merged = reference;
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
		cachedResources[(typeof(T), folder, file)] = new WeakReference(merged);
		ResourceLoaded?.Invoke(folder, file, merged, true);
		return merged;
	}
#endif

	public static (T reference, ResourceParser parser) Load<T>(string fullPath) where T : class
	{
		string text = null;
		byte[] bytes = null;
		foreach (var (parser, certainty) in RankParsers(fullPath, typeof(T)))
		{
			try
			{
				if (parser.OperateWith == OperateType.Text)
				{
					if (text == null)
						text = ReadText(fullPath);
					return text != null ? (parser.Read<T>(text, fullPath), parser) : default;
				}
				else
				{
					if (bytes == null)
						bytes = ReadBytes(fullPath);
					return bytes != null ? (parser.Read<T>(bytes, fullPath), parser) : default;
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
		foreach (var (parser, certainty) in RankParsers(fullPath, typeof(T)))
		{
			try
			{
				if (parser.OperateWith == OperateType.Text)
				{
					if (text == null)
						text = await ReadTextAsync(fullPath);
					return text != null ? (parser.Read<T>(text, fullPath), parser) : default;
				}
				else
				{
					if (bytes == null)
						bytes = await ReadBytesAsync(fullPath);
					return bytes != null ? (parser.Read<T>(bytes, fullPath), parser) : default;
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
	
		if (reference is UnityEngine.Object unityObject)
		{
			if (key.file == null || key.folder == ResourceFolder.Resources)
				Resources.UnloadAsset(unityObject);
			else
				UnityEngine.Object.Destroy(unityObject);
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

#if MODDING
		if (ModManager.Unload<T>(folder, file))
			return true;
#endif

		var key = (typeof(T), folder, file);
		if (cachedResources.TryGetValue(key, out WeakReference weakReference))
		{
			if (weakReference.Target is UnityEngine.Object unityObject)
			{
				if (folder == ResourceFolder.Resources)
					Resources.UnloadAsset(unityObject);
				else
					UnityEngine.Object.Destroy(unityObject);
			}
			cachedResources.Remove(key);
			ResourceUnloaded?.Invoke(folder, file);
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

	protected static UnityWebRequestAsyncOperation WebAsync(string filePath)
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
		foreach (var (parser, certainty) in RankParsers(fullPath, contents.GetType()))
		{
			try
			{
				if (parser.OperateWith == OperateType.Text)
					return SaveText(fullPath, (string) parser.Write(contents));
				else
					return SaveBytes(fullPath, (byte[]) parser.Write(contents));
			}
			catch (Exception)
			{
			}
		}
		return false;
	}

	public static UniTask<bool> SaveAsync(string fullPath, object contents)
	{
		foreach (var (parser, certainty) in RankParsers(fullPath, contents.GetType()))
		{
			try
			{
				if (parser.OperateWith == OperateType.Text)
					return SaveTextAsync(fullPath, (string) parser.Write(contents));
				else
					return SaveBytesAsync(fullPath, (byte[]) parser.Write(contents));
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
	protected static IEnumerable<(ResourceParser parser, float certainty)> RankParsers(string fullPath, Type type)
	{
		return ModManager.Parsers.Select(parser => (parser, certainty: parser.CanOperate(fullPath, type)))
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

	public static string LocalToURLPath(string path)
	{
		if (!path.Contains("file://"))
			path = "file://" + path;
		return path;
	}
	#endregion
}