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

	public static bool Modding = true;

	protected static Dictionary<string, object> CachedResources = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

	#region Loading
	public static T Load<T>(string fullPath) where T : class
	{
		if (!File.Exists(fullPath))
			return null;

		foreach (var (parser, certainty) in RankParsers<T>(fullPath))
		{
			if (parser.CanRead<T>(fullPath) >= 1.0f)
			{
				if (parser.OperateWith == OperateType.Text)
					return (T)parser.Read<T>(ReadText(fullPath), fullPath);
				else
					return (T)parser.Read<T>(ReadBytes(fullPath), fullPath);
			}
		}
		return null;
	}

	public static async Task<T> LoadAsync<T>(string fullPath) where T : class
	{
		foreach (var (parser, certainty) in RankParsers<T>(fullPath))
		{
			if (parser.CanRead<T>(fullPath) >= 1.0f)
			{
				if (parser.OperateWith == OperateType.Text)
				{
					string text = await ReadTextAsync(fullPath);
					return text != null ? (T) parser.Read<T>(text, fullPath) : null;
				}
				else
				{
					byte[] bytes = await ReadBytesAsync(fullPath);
					return bytes != null ? (T) parser.Read<T>(bytes, fullPath) : null;
				}
			}
		}
		return null;
	}

	protected static IEnumerable<(ResourceParser, float)> RankParsers<T>(string fullPath)
	{
		return ModManager.Parsers.Select(parser => (parser, certainty: parser.CanRead<T>(fullPath)))
			.Where(d => d.certainty > 0)
			.OrderByDescending(d => d.certainty);
	}

	public static T Load<T>(ResourceFolder folder, string file) where T : class
	{
		if (Modding)
		{
			T modded = ModManager.Load<T>(GetModdingPath(folder, file));
			if (modded != null)
				return modded;
		}

		return LoadCached<T>(folder, file);
	} 

	public static async Task<T> LoadAsync<T>(ResourceFolder folder, string file) where T : class
	{
		if (Modding)
		{
			T modded = await ModManager.LoadAsync<T>(GetModdingPath(folder, file));
			if (modded != null)
				return modded;
		}

		return await LoadCachedAsync<T>(folder, file);
	}

	protected static T LoadCached<T>(ResourceFolder folder, string file) where T : class
	{
		if (!CachedResources.TryGetValue(file, out object obj))
		{
			if (folder == ResourceFolder.Resources)
				obj = Resources.Load(Path.ChangeExtension(file, null));
			else
				obj = Load<T>(GetPath(folder, file));

			if (obj != null)
			{
				CachedResources.Add(file, obj);
				return (T) obj;
			}
		}
		return null;
	}

	protected static async Task<T> LoadCachedAsync<T>(ResourceFolder folder, string file) where T : class
	{
		if (!CachedResources.TryGetValue(file, out object obj))
		{
			if (folder == ResourceFolder.Resources)
				obj = await Resources.LoadAsync(Path.ChangeExtension(file, null));
			else
				obj = await LoadAsync<T>(GetPath(folder, file));

			if (obj != null)
			{
				CachedResources.Add(file, obj);
				return (T) obj;
			}
		}
		return null;
	}
	
	public static void Unload(object resource)
	{
		if (!Modding || !ModManager.Unload(resource))
		{
			foreach (KeyValuePair<string, object> kvp in CachedResources)
				if (kvp.Value == resource)
				{
					CachedResources.Remove(kvp.Key);
					break;
				}

			UnloadInternal(resource);
		}
	}

	public static void Unload(string path)
	{
		if (!Modding || !ModManager.Unload(path))
		{
			if (CachedResources.TryGetValue(path, out object resource))
			{
				CachedResources.Remove(path);
				UnloadInternal(resource);
			}		
		}
	}

	protected static void UnloadInternal(object resource)
	{
		if (resource is UnityEngine.Object)
		{
			try
			{
				UnityEngine.Object unityObject = (UnityEngine.Object)resource;

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
	}

	public static void ClearCache()
	{
		CachedResources.Clear();
		Resources.UnloadUnusedAssets();
	}
	#endregion

	#region Reading
	public static string ReadText(ResourceFolder folder, string file)
	{
		if (Modding)
		{
			string modded = ModManager.ReadText(GetModdingPath(folder, file));
			if (modded != null)
				return modded;
		}
		return ReadText(GetPath(folder, file));
	}

	public static async Task<string> ReadTextAsync(ResourceFolder folder, string file)
	{
		if (Modding)
		{
			string modded = await ModManager.ReadTextAsync(GetModdingPath(folder, file));
			if (modded != null)
				return modded;
		}
		return await ReadTextAsync(GetPath(folder, file));
	}

	public static byte[] ReadBytes(ResourceFolder folder, string file)
	{
		if (Modding)
		{
			byte[] modded = ModManager.ReadBytes(GetModdingPath(folder, file));
			if (modded != null)
				return modded;
		}
		return ReadBytes(GetPath(folder, file));
	}

	public static async Task<byte[]> ReadBytesAsync(ResourceFolder folder, string file)
	{
		if (Modding)
		{
			byte[] modded = await ModManager.ReadBytesAsync(GetModdingPath(folder, file));
			if (modded != null)
				return modded;
		}
		return await ReadBytesAsync(GetPath(folder, file));
	}

	public static string ReadText(string fullPath)
	{
		try
		{
			return File.ReadAllText(fullPath);
		}
		catch (Exception)
		{
			return null;
		}
	}

	public static async Task<string> ReadTextAsync(string fullPath)
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
		catch (Exception)
		{
			return null;
		}
	}

	public static async Task<byte[]> ReadBytesAsync(string fullPath)
	{
		UnityWebRequest request = await WebAsync(fullPath);
		if (request.isHttpError || request.isNetworkError)
			return null;
		else
			return request.downloadHandler.data;
	}

	public static async Task<UnityWebRequest> WebAsync(string filePath)
	{
		UnityWebRequest request = UnityWebRequest.Get(LocalToURLPath(filePath));
		await request.SendWebRequest();
		return request;
	}

	// TODO: Data merging with ResourceParser
	/*
	public static T Read<T>(ResourceFolder folder, string file)
    {	
		if (merge)
		{
			List<string> contents = new List<string>();
			string fullPath = GetPath(folder, file);
			if (Exists(fullPath))
				contents.Add(Read(fullPath));

			if (Modding)
				contents.AddRange(ModManager.ReadTextAll(file));

			if (contents.Count > 0)
				return GetMerged<T>(contents);

			return default;
		}
		else
		{
			if (Modding)
			{
				string modded = ModManager.ReadText(file);
				if (modded != null)
					return DecodeObject<T>(modded);
			}
			return Read<T>((GetPath(folder, file)));
		}
    }

	public static async Task<T> ReadAsync<T>(ResourceFolder folder, string file)
	{
		if (merge)
		{
			List<string> contents = new List<string>();
			string fullPath = GetPath(folder, file);
			if (Exists(fullPath))
				contents.Add(await ReadAsync(fullPath));

			if (Modding)
				contents.AddRange(await ModManager.ReadTextAllAsync(file));

			if (contents.Count > 0)
				return GetMerged<T>(contents);

			return default;
		}
		else
		{
			if (Modding)
			{
				string modded = await ModManager.ReadTextAsync(file);
				if (modded != null)
					return DecodeObject<T>(modded);
			}

			return await ReadAsync<T>(GetPath(folder, file));
		}
	}

	protected static T GetMerged<T>(List<string> contents)
	{
		T current = DecodeObject<T>(contents[0]);
		for (int i = 1; i < contents.Count; i++)
			OverwriteObject(current, contents[i]);
		return current;
	}
	*/
	#endregion

	#region Saving/Deleting
	public static bool Save(ResourceFolder folder, string file, object contents, ResourceParser parser)
	{
		return Save(GetPath(folder, file), contents, parser);
	}

	public static async Task<bool> SaveAsync(ResourceFolder folder, string file, object contents, ResourceParser parser)
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

	public static async Task<bool> SaveAsync(string fullPath, object contents, ResourceParser parser)
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

	public static async Task<bool> SaveTextAsync(ResourceFolder folder, string file, string contents)
	{
		return await SaveTextAsync(GetPath(folder, file), contents);
	}

	public static bool SaveBytes(ResourceFolder folder, string file, byte[] bytes)
	{
		return SaveBytes(GetPath(folder, file), bytes);
	}

	public static async Task<bool> SaveBytesAsync(ResourceFolder folder, string file, byte[] bytes)
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
		catch (Exception)
		{
			return false;
		}
	}

	public static async Task<bool> SaveTextAsync(string fullPath, string contents)
	{
		try
		{
			using (StreamWriter stream = new StreamWriter(fullPath))
				await stream.WriteAsync(contents);
			return true;
		}
		catch (Exception)
		{
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
		catch (Exception)
		{
			return false;
		}
	}

	public static async Task<bool> SaveBytesAsync(string fullPath, byte[] bytes)
	{
		try
		{
			using (FileStream stream = new FileStream(fullPath, FileMode.Create))
				await stream.WriteAsync(bytes, 0, bytes.Length);
			return true;
		}
		catch (Exception)
		{
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
		catch (Exception)
		{
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
	public static string GetPath(ResourceFolder folder)
	{
		return Paths[folder];
	}

	public static string GetPath(ResourceFolder folder, string file)
	{
		return Path.Combine(GetPath(folder), file);
	}

	public static string GetModdingPath(ResourceFolder folder, string file)
	{
		return Path.Combine(folder.ToString(), file);
	}

	public static string LocalToURLPath(string path)
	{
		if (!path.Contains("file://"))
			path = "file://" + path;
		return path;
	}
	#endregion
}