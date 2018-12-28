﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
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
}

public class ResourceManager
{
	public static Dictionary<ResourceFolder, string> Paths = new Dictionary<ResourceFolder, string>
	{	{ ResourceFolder.Data, Application.dataPath },
		{ ResourceFolder.StreamingAssets, Application.streamingAssetsPath },
		{ ResourceFolder.PersistentData, Application.persistentDataPath } };

	public static bool Modding = true;

	protected static Dictionary<string, object> CachedResources = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

	#region Loading
	public static T Load<T>(string path)
	{
		if (Modding)
		{
			T modded = ModManager.Load<T>(path);
			if (modded != null)
				return modded;
		}
		
		return LoadCached<T>(Path.ChangeExtension(path, null));
	} 

	public static async Task<T> LoadAsync<T>(string path)
	{
		if (Modding)
		{
			T modded = await ModManager.LoadAsync<T>(path);
			if (modded != null)
				return modded;
		}

		return await LoadCachedAsync<T>(Path.ChangeExtension(path, null));
	}

	public static T LoadCached<T>(string filePath)
	{
		if (!CachedResources.TryGetValue(filePath, out object obj))
		{
			obj = Resources.Load(filePath);
			CachedResources.Add(filePath, obj);
			return (T) obj;
		}
		return default;
	}

	public static async Task<T> LoadCachedAsync<T>(string filePath) 
	{
		if (!CachedResources.TryGetValue(filePath, out object obj))
		{
			obj = await Resources.LoadAsync(filePath);
			CachedResources.Add(filePath, obj);
			return (T) obj;
		}
		return default;
	}
	
	public static void Unload(object asset)
	{
		if (!Modding || !ModManager.Unload(asset))
		{
			foreach (KeyValuePair<string, object> kvp in CachedResources)
				if (kvp.Value == asset)
				{
					CachedResources.Remove(kvp.Key);
					break;
				}
		}

		Resources.UnloadAsset((UnityEngine.Object) asset);
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
			string modded = ModManager.ReadText(file);
			if (modded != null)
				return modded;
		}
		return ReadText(GetPath(folder, file));
	}

	public static async Task<string> ReadTextAsync(ResourceFolder folder, string file)
	{
		if (Modding)
		{
			string modded = await ModManager.ReadTextAsync(file);
			if (modded != null)
				return modded;
		}
		return await ReadTextAsync(GetPath(folder, file));
	}

	public static byte[] ReadBytes(ResourceFolder folder, string file)
	{
		if (Modding)
		{
			byte[] modded = ModManager.ReadBytes(file);
			if (modded != null)
				return modded;
		}
		return ReadBytes(GetPath(folder, file));
	}

	public static async Task<byte[]> ReadBytesAsync(ResourceFolder folder, string file)
	{
		if (Modding)
		{
			byte[] modded = await ModManager.ReadBytesAsync(file);
			if (modded != null)
				return modded;
		}
		return await ReadBytesAsync(GetPath(folder, file));
	}

	public static string ReadText(string fullPath)
	{
		return File.ReadAllText(fullPath);
	}

	public static async Task<string> ReadTextAsync(string fullPath)
	{
		return (await LoadAsync(fullPath)).downloadHandler.text;
	}

	public static byte[] ReadBytes(string fullPath)
	{
		return File.ReadAllBytes(fullPath);
	}

	public static async Task<byte[]> ReadBytesAsync(string fullPath)
	{
		return (await LoadAsync(fullPath)).downloadHandler.data;
	}

	public static async Task<UnityWebRequest> LoadAsync(string filePath)
	{
		UnityWebRequest request = UnityWebRequest.Get(LocalToURLPath(filePath));
		await request.SendWebRequest();
		return request;
	}

	public static T Read<T>(string fullPath)
	{
		foreach (ResourceParser parser in ModManager.Parsers)
		{
			if (parser.CanRead<T>(fullPath))
			{
				T obj = default;
				if (parser.OperateWith == OperateType.Text)
					obj = (T) parser.Read<T>(ReadText(fullPath));
				else
					obj = (T) parser.Read<T>(ReadBytes(fullPath));
				if (obj != null)
					return obj;
			}
		}
		return default;
	}

	public static async Task<T> ReadAsync<T>(string fullPath)
	{
		foreach (ResourceParser parser in ModManager.Parsers)
		{
			if (parser.CanRead<T>(fullPath))
			{
				T obj = default;
				if (parser.OperateWith == OperateType.Text)
					obj = (T) parser.Read<T>(await ReadTextAsync(fullPath));
				else
					obj = (T) parser.Read<T>(await ReadBytesAsync(fullPath));
				if (obj != null)
					return obj;
			}
		}
		return default;
	}

	public static T Read<T>(ResourceFolder folder, string file)
    {	
		if (Modding)
		{
			T modded = ModManager.Load<T>(file);
			if (modded != null)
				return modded;
		}
		return Read<T>((GetPath(folder, file)));


		/*
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
		*/
    }

	public static async Task<T> ReadAsync<T>(ResourceFolder folder, string file)
	{
		if (Modding)
		{
			T modded = await ModManager.LoadAsync<T>(file);
			if (modded != null)
				return modded;
		}

		return await ReadAsync<T>(GetPath(folder, file));
		/*
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
		*/
	}

	/*
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
	public static void Save(ResourceFolder folder, string file, object contents, ResourceParser parser)
	{
		Save(GetPath(folder, file), contents, parser);
	}

	public static async Task SaveAsync(ResourceFolder folder, string file, object contents, ResourceParser parser)
	{
		await SaveAsync(GetPath(folder, file), contents, parser);
	}

	public static void Save(string fullPath, object contents, ResourceParser parser)
	{
		if (parser.OperateWith == OperateType.Text)
			SaveText(fullPath, (string)parser.Write(contents));
		else
			SaveBytes(fullPath, (byte[])parser.Write(contents));
	}

	public static async Task SaveAsync(string fullPath, object contents, ResourceParser parser)
	{
		if (parser.OperateWith == OperateType.Text)
			await SaveTextAsync(fullPath, (string)parser.Write(contents));
		else
			await SaveBytesAsync(fullPath, (byte[])parser.Write(contents));
	}

	public static void SaveText(ResourceFolder folder, string file, string contents)
	{
		SaveText(GetPath(folder, file), contents);
	}

	public static async Task SaveTextAsync(ResourceFolder folder, string file, string contents)
	{
		await SaveTextAsync(GetPath(folder, file), contents);
	}

	public static void SaveBytes(ResourceFolder folder, string file, byte[] bytes)
	{
		SaveBytes(GetPath(folder, file), bytes);
	}

	public static async Task SaveBytesAsync(ResourceFolder folder, string file, byte[] bytes)
	{
		await SaveBytesAsync(GetPath(folder, file), bytes);
	}

	public static void SaveText(string fullPath, string contents)
	{
		File.WriteAllText(fullPath, contents);
	}

	public static async Task SaveTextAsync(string fullPath, string contents)
	{
		using (StreamWriter stream = new StreamWriter(fullPath))
			await stream.WriteAsync(contents);
	}

	public static void SaveBytes(string fullPath, byte[] bytes)
	{
		File.WriteAllBytes(fullPath, bytes);
	}

	public static async Task SaveBytesAsync(string fullPath, byte[] bytes)
	{
		using (FileStream stream = new FileStream(fullPath, FileMode.Create))
			await stream.WriteAsync(bytes, 0, bytes.Length);
	}

	public static void Delete(ResourceFolder folder, string file)
	{
		Delete(GetPath(folder, file));
	}

	public static void Delete(string fullPath)
	{
		File.Delete(fullPath);
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

	public static string LocalToURLPath(string path)
	{
		if (!path.Contains("file://"))
			path = "file://" + path;
		return path;
	}
	#endregion
}