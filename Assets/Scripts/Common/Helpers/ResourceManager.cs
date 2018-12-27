using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Modding;
using System.Threading.Tasks;
using UniRx.Async;

public enum ResourceFolder
{
	Data,
	StreamingAssets,
	PersistentData,
}

public class ResourceManager
{
	public static Dictionary<ResourceFolder, string> Paths = new Dictionary<ResourceFolder, string>
	{	{ ResourceFolder.Data, Application.dataPath },
		{ ResourceFolder.StreamingAssets, Application.streamingAssetsPath },
		{ ResourceFolder.PersistentData, Application.persistentDataPath } };

	public static bool Modding = true;

	protected static Dictionary<string, UnityEngine.Object> CachedResources = new Dictionary<string, UnityEngine.Object>();

	public static T Load<T>(string path) where T : UnityEngine.Object
	{
		if (Modding)
		{
			T modded = ModManager.Load<T>(path);
			if (modded != null)
				return modded;
		}
		
		return LoadCached<T>(Path.ChangeExtension(path, null));
	} 

	public static async Task<T> LoadAsync<T>(string path) where T : UnityEngine.Object
	{
		if (Modding)
		{
			T modded = await ModManager.LoadAsync<T>(path);
			if (modded != null)
				return modded;
		}

		return await LoadCachedAsync<T>(Path.ChangeExtension(path, null));
	}

	public static T LoadCached<T>(string filePath) where T : UnityEngine.Object
	{
		UnityEngine.Object obj = null;
		if (!CachedResources.TryGetValue(filePath, out obj))
		{
			obj = Resources.Load<T>(filePath);
			CachedResources.Add(filePath, obj);
		}
		return obj as T;
	}

	public static async Task<T> LoadCachedAsync<T>(string filePath) where T : UnityEngine.Object
	{
		UnityEngine.Object obj = null;
		if (!CachedResources.TryGetValue(filePath, out obj))
		{
			obj = await Resources.LoadAsync<T>(filePath);
			CachedResources.Add(filePath, obj);
		}
		return obj as T;
	}
	
	public static void Unload(UnityEngine.Object asset)
	{
		if (!Modding || !ModManager.Unload(asset))
		{
			foreach (KeyValuePair<string, UnityEngine.Object> kvp in CachedResources)
				if (kvp.Value == asset)
				{
					CachedResources.Remove(kvp.Key);
					break;
				}
		}

		Resources.UnloadAsset(asset);
	}

	public static void ClearCache()
	{
		CachedResources.Clear();
		Resources.UnloadUnusedAssets();
	}

	public static string Read(ResourceFolder folder, string file)
    {
		if (Modding)
		{
			string modded = ModManager.ReadText(file);
			if (modded != null)
				return modded;
		}
        return Read(GetPath(folder, file));
    }
	
    public static T Read<T>(ResourceFolder folder, string file, bool merge = false)
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

			return default(T);
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

	public static T Read<T>(string fullPath)
	{
		return DecodeObject<T>(Read(fullPath));
	}

	public static string Read(string fullPath)
	{
		return File.ReadAllText(fullPath);
	}

	protected static T GetMerged<T>(List<string> contents)
	{
		T current = DecodeObject<T>(contents[0]);
		for (int i = 1; i < contents.Count; i++)
			OverwriteObject(current, contents[i]);
		return current;
	}

	public static async Task<T> ReadAsync<T>(ResourceFolder folder, string file, bool merge = false)
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

			return default(T);
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

	public static async Task<string> ReadAsync(ResourceFolder folder, string file)
	{
		if (Modding)
		{
			string modded = await ModManager.ReadTextAsync(file);
			if (modded != null)
				return modded;
		}
		return await ReadAsync(GetPath(folder, file));
	}

	public static async Task<T> ReadAsync<T>(string fullPath)
	{
		return DecodeObject<T>(await ReadAsync(fullPath));
	}

	public static async Task<string> ReadAsync(string fullPath)
	{
		return (await LoadAsync(fullPath)).downloadHandler.text;
	}
	
	public static async Task<UnityWebRequest> LoadAsync(string filePath)
	{
		UnityWebRequest request = UnityWebRequest.Get(LocalToURLPath(filePath));
		await request.SendWebRequest();
		return request;
	}

	public static void Save(ResourceFolder folder, string file, string contents)
	{
		Save(GetPath(folder, file), contents);
	}

	public static async Task SaveAsync(ResourceFolder folder, string file, string contents)
	{
		await SaveAsync(GetPath(folder, file), contents);
	}

	public static void Save(ResourceFolder folder, string file, object contents)
	{
		Save(GetPath(folder, file), contents);
	}

	public static async Task SaveAsync(ResourceFolder folder, string file, object contents)
	{
		await SaveAsync(GetPath(folder, file), contents);
	}

	public static void Save(string fullPath, object contents)
	{
		Save(fullPath, EncodeObject(contents));
	}

	public static async Task SaveAsync(string fullPath, object contents)
	{
		await SaveAsync(fullPath, EncodeObject(contents));
	}

	public static void Save(string fullPath, string contents)
	{
		File.WriteAllText(fullPath, contents);
	}

	public static async Task SaveAsync(string fullPath, string contents)
	{
		using (StreamWriter stream = new StreamWriter(fullPath))
			await stream.WriteAsync(contents);
	}

	public static bool Exists(ResourceFolder folder, string file)
	{
		return Exists(GetPath(folder, file));
	}

	public static bool Exists(string fullPath)
	{
		return File.Exists(fullPath);
	}

	public static void Delete(ResourceFolder folder, string file)
	{
		Delete(GetPath(folder, file));
	}

	public static void Delete(string fullPath)
	{
		File.Delete(fullPath);
	}

	public static string GetPath(ResourceFolder folder)
	{
		return Paths[folder];
	}

	public static string GetPath(ResourceFolder folder, string file)
	{
		return Path.Combine(GetPath(folder), file);
	}

	public static T DecodeObject<T>(string encoded)
	{
		return JsonUtility.FromJson<T>(encoded);
	}

	public static string EncodeObject(object data)
	{
		return JsonUtility.ToJson(data, true);
	}

	public static void OverwriteObject(object data, string overwrite)
	{
		JsonUtility.FromJsonOverwrite(overwrite, data);
	}

	public static string LocalToURLPath(string path)
	{
		if (!path.Contains("file://"))
			path = "file://" + path;
		return path;
	}
}
