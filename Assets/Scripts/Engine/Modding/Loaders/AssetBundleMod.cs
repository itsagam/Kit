#if MODDING
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Engine.Parsers;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Engine.Modding.Loaders
{
	public class AssetBundleModLoader: ModLoader
	{
		public readonly List<string> SupportedExtensions = new List<string> { ".assetbundle", ".unity3d" };

		public override Mod LoadMod(string path)
		{
			FileAttributes attributes = File.GetAttributes(path);
			if (attributes.HasFlag(FileAttributes.Directory))
				return null;

			if (!ResourceManager.MatchExtension(path, SupportedExtensions))
				return null;

			try
			{
				AssetBundle bundle = AssetBundle.LoadFromFile(path);
				if (bundle == null)
				{
					Debugger.Log("ModManager", $"AssetBundle could not be loaded for mod \"{path}\"");
					return null;
				}

				AssetBundleMod mod = new AssetBundleMod(bundle);
				ModMetadata metadata = mod.Load<ModMetadata>(MetadataFile);
				if (metadata == null)
				{
					Debugger.Log("ModManager", $"Could not load metadata for mod \"{path}\"");
					return null;
				}
				mod.Metadata = metadata;
				return mod;
			}
			catch (Exception ex)
			{
				Debugger.Log("ModManager", $"Error loading mod \"{path}\" – {ex.Message}");
				return null;
			}
		}

		public override async UniTask<Mod> LoadModAsync(string path)
		{
			FileAttributes attributes = File.GetAttributes(path);
			if (attributes.HasFlag(FileAttributes.Directory))
				return null;

			if (!ResourceManager.MatchExtension(path, SupportedExtensions))
				return null;

			try
			{
				AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);
				await request;
				if (request.assetBundle == null)
				{
					Debugger.Log("ModManager", $"AssetBundle could not be loaded for mod \"{path}\"");
					return null;
				}

				AssetBundleMod mod = new AssetBundleMod(request.assetBundle);
				ModMetadata metadata = await mod.LoadAsync<ModMetadata>(MetadataFile);
				if (metadata == null)
				{
					Debugger.Log("ModManager", $"Could not load metadata for mod \"{path}\"");
					return null;
				}
				mod.Metadata = metadata;
				return mod;
			}
			catch (Exception ex)
			{
				Debugger.Log("ModManager", $"Error loading mod \"{path}\" – {ex.Message}");
				return null;
			}
		}
	}

	public class AssetBundleMod: Mod
	{
		public AssetBundle Bundle { get; }

		public AssetBundleMod(AssetBundle bundle)
		{
			Bundle = bundle;
		}

		public override (object reference, string filePath, ResourceParser parser) LoadEx(Type type, string path)
		{
			// If input type is UnityEngine.Object, load the asset from bundle locally otherwise try to parse
			return typeof(Object).IsAssignableFrom(type) ? LoadUnityObject(type, path) : base.LoadEx(type, path);
		}

		private (object reference, string filePath, ResourceParser parser) LoadUnityObject(Type type, string path)
		{
			try
			{
				string matchingFile = FindFiles(path).First();
				object reference = Bundle.LoadAsset(matchingFile, type);
				if (reference != null)
					return (reference, matchingFile, null);
			}
			catch
			{
			}
			return default;
		}


		public override UniTask<(object reference, string filePath, ResourceParser parser)> LoadExAsync(Type type, string path)
		{
			return typeof(Object).IsAssignableFrom(type) ? LoadUnityObjectAsync(type, path) : base.LoadExAsync(type, path);
		}

		private async UniTask<(object reference, string filePath, ResourceParser parser)> LoadUnityObjectAsync(Type type, string path)
		{
			try
			{
				string matchingFile = FindFiles(path).First();
				AssetBundleRequest request = Bundle.LoadAssetAsync(matchingFile, type);
				await request;
				if (request.asset != null)
					return (request.asset, matchingFile, null);
			}
			catch
			{
			}
			return default;
		}

		public override string ReadText(string path)
		{
			TextAsset textAsset = (TextAsset) LoadUnityObject(typeof(TextAsset), path).reference;
			return textAsset != null ? textAsset.text : null;
		}

		public override async UniTask<string> ReadTextAsync(string path)
		{
			TextAsset textAsset = (TextAsset) (await LoadUnityObjectAsync(typeof(TextAsset), path)).reference;
			return textAsset != null ? textAsset.text : null;
		}

		public override byte[] ReadBytes(string path)
		{
			TextAsset textAsset = (TextAsset) LoadUnityObject(typeof(TextAsset), path).reference;
			return textAsset != null ? textAsset.bytes : null;
		}

		public override async UniTask<byte[]> ReadBytesAsync(string path)
		{
			TextAsset textAsset = (TextAsset) (await LoadUnityObjectAsync(typeof(TextAsset), path)).reference;
			return textAsset != null ? textAsset.bytes : null;
		}

		public override IEnumerable<string> FindFiles(string path)
		{
			path = GetAssetPath(path);
			if (Bundle.Contains(path))
				return EnumerableExtensions.One(path);

			if (Path.HasExtension(path))
				return Enumerable.Empty<string>();

			return Bundle.GetAllAssetNames()
			             .Where(assetPath => ResourceManager.ComparePath(path, Path.ChangeExtension(assetPath, null)));
		}

		public override bool Exists(string path)
		{
			return Bundle.Contains(GetAssetPath(path));
		}

		// AssetBundle paths include "Assets/" and file extensions. We add "Assets/" to path if it doesn't already contain it
		// because when we load non-UnityObjects LoadEx -> FindFiles -> ReadText -> LoadUnityObject -> FindFiles can create a
		// chain of commands where "Assets/" is appended multiple times.
		private static string GetAssetPath(string path)
		{
			return path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) ? path : "Assets/" + path;
		}

		public override void Unload()
		{
			base.Unload();
			Bundle.Unload(false);
		}
	}
}
#endif