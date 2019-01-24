using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UniRx.Async;
using Modding;
using Modding.Parsers;

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
	{   { ResourceFolder.Data, Application.dataPath },
		{ ResourceFolder.StreamingAssets, Application.streamingAssetsPath },
		{ ResourceFolder.PersistentData, Application.persistentDataPath },
		{ ResourceFolder.Resources, Path.Combine(Application.dataPath, "Resources") } };

	// Global variable to enable/disable modding
	public static bool Modding = true;
	// Default mode for modding in individual calls
	public const bool DefaultModding = true;

	public static event Action<ResourceFolder, string, object> ResourceLoaded;
	public static event Action<ResourceFolder, string, object> ResourceReused;

	protected static Dictionary<string, object> resources = new Dictionary<string, object>();
	protected static Dictionary<ResourceFolder, string> folderToString = new Dictionary<ResourceFolder, string>();

	static ResourceManager()
	{
		foreach (var kvp in Paths)
			folderToString[kvp.Key] = kvp.Key.ToString();
	}

	#region Loading
	public static T Load<T>(ResourceFolder folder, string file, bool modded = DefaultModding, bool merge = false) where T : class
	{
		if (Modding && modded)
		{
			if (!merge)
			{
				string moddingPath = GetModdingPath(folder, file);
				T moddedFile = ModManager.Load<T>(moddingPath);
				if (moddedFile != null)
					return moddedFile;
			}
			else
				return LoadMerged<T>(folder, file);
		}

		return LoadUnmodded<T>(folder, file);
	}

	public static async UniTask<T> LoadAsync<T>(ResourceFolder folder, string file, bool modded = DefaultModding, bool merge = false) where T : class
	{
		if (Modding && modded)
		{
			if (!merge)
			{
				string moddingPath = GetModdingPath(folder, file);
				T moddedFile = await ModManager.LoadAsync<T>(moddingPath);
				if (moddedFile != null)
					return moddedFile;
			}
			else
				return await LoadMergedAsync<T>(folder, file);
		}

		return await LoadUnmoddedAsync<T>(folder, file);
	}

	// TODO: Handle conflict between loading same filename from two different ResourceFolder
	public static T LoadUnmodded<T>(ResourceFolder folder, string file) where T : class
	{
		object reference = null;
		if (!resources.TryGetValue(file, out reference))
		{
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
				resources.Add(file, reference);
				ResourceLoaded?.Invoke(folder, file, reference);
			}
		}
		else
			ResourceReused?.Invoke(folder, file, reference);
		return (T) reference;
	}

	public static async UniTask<T> LoadUnmoddedAsync<T>(ResourceFolder folder, string file) where T : class
	{
		object reference = null;
		if (!resources.TryGetValue(file, out reference))
		{
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
				resources.Add(file, reference);
				ResourceLoaded?.Invoke(folder, file, reference);
			}
		}
		else
			ResourceReused?.Invoke(folder, file, reference);
		return (T) reference;
	}

	public static T LoadMerged<T>(ResourceFolder folder, string file) where T : class
	{
		object reference = null;
		if (resources.TryGetValue(file, out reference))
			return (T) reference;

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
				string moddingPath = GetModdingPath(folder, file);
				if (parser.OperateWith == OperateType.Text)
				{
					List<string> textList = ModManager.ReadTextAll(moddingPath);
					foreach (string text in textList)
						parser.Merge<T>(merged, text);
				}
				else
				{
					List<byte[]> bytesList = ModManager.ReadBytesAll(moddingPath);
					foreach (byte[] bytes in bytesList)
						parser.Merge<T>(merged, bytes);
				}
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}
		resources[file] = merged;
		return merged;
	}

	public static async UniTask<T> LoadMergedAsync<T>(ResourceFolder folder, string file) where T : class
	{
		object reference = null;
		if (resources.TryGetValue(file, out reference))
			return (T) reference;

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
				string moddingPath = GetModdingPath(folder, file);
				if (parser.OperateWith == OperateType.Text)
				{
					List<string> textList = await ModManager.ReadTextAllAsync(moddingPath);
					foreach (string text in textList)
						parser.Merge<T>(merged, text);
				}
				else
				{
					List<byte[]> bytesList = await ModManager.ReadBytesAllAsync(moddingPath);
					foreach (byte[] bytes in bytesList)
						parser.Merge<T>(merged, bytes);
				}
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}
		resources[file] = merged;
		return merged;
	}

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
		if (Modding && ModManager.Unload(resource))
			return true;

		bool found = false;
		foreach (var kvp in resources.Reverse())
			if (kvp.Value == resource)
			{
				resources.Remove(kvp.Key);
				found = true;
				break;
			}

		if (resource is UnityEngine.Object unityObject)
		{
			try
			{
				// GetInstanceID() seems to return positive values for loaded assets and negative for created ones
				if (unityObject.GetInstanceID() > 0)
					Resources.UnloadAsset(unityObject);
				else
					UnityEngine.Object.Destroy(unityObject);
			}
			catch (Exception)
			{
			}
		}
		return found;
	}

	public static bool Unload(ResourceFolder folder, string file)
	{
		string moddingPath = GetModdingPath(folder, file);
		if (Modding && ModManager.UnloadAll(moddingPath))
			return true;
		
		if (resources.TryGetValue(file, out object resource))
		{
			resources.Remove(file);
			if (resource is UnityEngine.Object unityObject)
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
		if (Modding && modded)
		{
			string moddedFile = ModManager.ReadText(GetModdingPath(folder, file));
			if (moddedFile != null)
				return moddedFile;
		}
		return ReadText(GetPath(folder, file));
	}

	public static async UniTask<string> ReadTextAsync(ResourceFolder folder, string file, bool modded = DefaultModding)
	{
		if (Modding && modded)
		{
			string moddedFile = await ModManager.ReadTextAsync(GetModdingPath(folder, file));
			if (moddedFile != null)
				return moddedFile;
		}
		return await ReadTextAsync(GetPath(folder, file));
	}

	public static byte[] ReadBytes(ResourceFolder folder, string file, bool modded = DefaultModding)
	{
		if (Modding && modded)
		{
			byte[] moddedFile = ModManager.ReadBytes(GetModdingPath(folder, file));
			if (moddedFile != null)
				return moddedFile;
		}
		return ReadBytes(GetPath(folder, file));
	}

	public static async UniTask<byte[]> ReadBytesAsync(ResourceFolder folder, string file, bool modded = DefaultModding)
	{
		if (Modding && modded)
		{
			byte[] moddedFile = await ModManager.ReadBytesAsync(GetModdingPath(folder, file));
			if (moddedFile != null)
				return moddedFile;
		}
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
			Debug.LogException(e);
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
			Debug.LogException(e);
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
			Debug.LogException(e);
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
			Debug.LogException(e);
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
			Debug.LogException(e);
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
			Debug.LogException(e);
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
			Debug.LogException(e);
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
	public static List<ResourceInfo> GetModdedResourceInfo(ResourceFolder folder, string file)
	{
		return ModManager.GetResourceInfo(GetModdingPath(folder, file));
	}

	public static string GetPath(ResourceFolder folder)
	{
		return Paths[folder];
	}

	public static string GetPath(ResourceFolder folder, string file)
	{
		return GetPath(folder) + "/" + file;
	}

	public static string GetModdingPath(ResourceFolder folder)
	{
		return folderToString[folder];
	}

	public static string GetModdingPath(ResourceFolder folder, string file)
	{
		return GetModdingPath(folder) + "/" + file;
	}

	public static string LocalToURLPath(string path)
	{
		if (!path.Contains("file://"))
			path = "file://" + path;
		return path;
	}
	#endregion
}