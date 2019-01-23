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

	// TODO: StringComparer.OrdinalIgnoreCase is slowing down caching
	protected static Dictionary<string, object> resources = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

	#region Loading
	public static T Load<T>(ResourceFolder folder, string file, bool modded = DefaultModding, bool merge = false) where T : class
	{
		return LoadInternal<T>(folder, file, modded, merge, false).Result;
	}

	public static async Task<T> LoadAsync<T>(ResourceFolder folder, string file, bool modded = DefaultModding, bool merge = false) where T : class
	{
		return await LoadInternal<T>(folder, file, modded, merge, true);
	}

	protected static async Task<T> LoadInternal<T>(ResourceFolder folder, string file, bool modded, bool merge, bool async) where T : class
	{
		if (Modding && modded)
		{
			if (!merge)
			{
				string moddingPath = GetModdingPath(folder, file);
				T moddedFile = async ? await ModManager.LoadAsync<T>(moddingPath) : ModManager.Load<T>(moddingPath);
				if (moddedFile != null)
					return moddedFile;
			}
			else
				return async ? await LoadMergedAsync<T>(folder, file) : LoadMerged<T>(folder, file);
		}

		return async ? await LoadUnmoddedAsync<T>(folder, file) : LoadUnmodded<T>(folder, file);
	}

	public static T LoadUnmodded<T>(ResourceFolder folder, string file) where T : class
	{
		return LoadUnmoddedInternal<T>(folder, file, false).Result;
	}

	public static async Task<T> LoadUnmoddedAsync<T>(ResourceFolder folder, string file) where T : class
	{
		return await LoadUnmoddedInternal<T>(folder, file, true);
	}

	// TODO: Tasks are slowing down sync methods (along with StringComparer.OrdinalIgnoreCase) -- use UniTask from UniRx?
	protected static async Task<T> LoadUnmoddedInternal<T>(ResourceFolder folder, string file, bool async) where T : class
	{
		object reference = null;
		if (!resources.TryGetValue(file, out reference))
		{
			if (folder == ResourceFolder.Resources)
			{
				string fileNoExt = Path.ChangeExtension(file, null);
				reference = async ? await Resources.LoadAsync(fileNoExt) : Resources.Load(fileNoExt);
			}
			else
			{
				string fullPath = GetPath(folder, file);
				reference = async ? (await LoadAsync<T>(fullPath)).reference : Load<T>(fullPath).reference;
			}

			if (reference != null)
				resources.Add(file, reference);
		}
		return (T) reference;
	}

	public static T LoadMerged<T>(ResourceFolder folder, string file) where T : class
	{
		return LoadMergedInternal<T>(folder, file, false).Result;
	}

	public static async Task<T> LoadMergedAsync<T>(ResourceFolder folder, string file) where T : class
	{
		return await LoadMergedInternal<T>(folder, file, true);
	}

	protected static async Task<T> LoadMergedInternal<T>(ResourceFolder folder, string file, bool async) where T : class
	{
		object reference = null;
		if (resources.TryGetValue(file, out reference))
			return (T) reference;

		ResourceParser parser = null;
		string fullPath = GetPath(folder, file);
		if (folder == ResourceFolder.Resources)
		{
			string fileNoExt = Path.ChangeExtension(file, null);
			reference = async ? await Resources.LoadAsync(fileNoExt) : Resources.Load(fileNoExt);
			if (reference == null)
				return null;
			parser = RankParsers<T>(fullPath).FirstOrDefault().parser;
		}
		else
		{
			(reference, parser) = async ? await LoadAsync<T>(fullPath) : Load<T>(fullPath);
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
					List<string> textList = async ? await ModManager.ReadTextAllAsync(moddingPath) : ModManager.ReadTextAll(moddingPath);
					foreach (string text in textList)
						parser.Merge<T>(merged, text);
				}
				else
				{
					List<byte[]> bytesList = async ? await ModManager.ReadBytesAllAsync(moddingPath) : ModManager.ReadBytesAll(moddingPath);
					foreach (byte[] bytes in bytesList)
						parser.Merge<T>(merged, bytes);
				}
			}
			catch (Exception)
			{
			}
		}
		resources[file] = merged;
		return merged;
	}

	public static (T reference, ResourceParser parser) Load<T>(string fullPath) where T : class
	{
		return LoadInternal<T>(fullPath, false).Result;
	}

	public static async Task<(T reference, ResourceParser parser)> LoadAsync<T>(string fullPath) where T : class
	{
		return await LoadInternal<T>(fullPath, true);
	}

	protected static async Task<(T reference, ResourceParser parser)> LoadInternal<T>(string fullPath, bool async) where T : class
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
						text = async ? await ReadTextAsync(fullPath) : ReadText(fullPath);
					return text != null ? ((T) parser.Read<T>(text, fullPath), parser) : (null, null);
				}
				else
				{
					if (bytes == null)
						bytes = async ? await ReadBytesAsync(fullPath) : ReadBytes(fullPath);
					return bytes != null ? ((T) parser.Read<T>(bytes, fullPath), parser) : (null, null);
				}
			}
			catch (Exception)
			{
			}
		}
		return (null, null);
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

	public static async Task<string> ReadTextAsync(ResourceFolder folder, string file, bool modded = DefaultModding)
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

	public static async Task<byte[]> ReadBytesAsync(ResourceFolder folder, string file, bool modded = DefaultModding)
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
		return Path.Combine(GetPath(folder), file);
	}

	public static string GetModdingPath(ResourceFolder folder)
	{
		return folder.ToString();
	}

	public static string GetModdingPath(ResourceFolder folder, string file)
	{
		return Path.Combine(GetModdingPath(folder), file);
	}

	public static string LocalToURLPath(string path)
	{
		if (!path.Contains("file://"))
			path = "file://" + path;
		return path;
	}
	#endregion
}