#if MODDING
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Engine.Parsers;
using UniRx.Async;
using UnityEngine;

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
				if (bundle != null)
				{
					AssetBundleMod mod = new AssetBundleMod(bundle);
					if (mod.LoadMetadata())
						return mod;
				}
			}
			catch (Exception ex)
			{
				Debugger.Log("ModManager", $"Error loading mod \"{path}\" – {ex.Message}");
			}

			return null;
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
				AssetBundle bundle = null;
				await LoadLocal();
				IEnumerator LoadLocal()
				{
					AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);
					yield return request;
					bundle = request.assetBundle;
				}
				if (bundle != null)
				{
					AssetBundleMod mod = new AssetBundleMod(bundle);
					if (await mod.LoadMetadataAsync())
						return mod;
				}
			}
			catch (Exception ex)
			{
				Debugger.Log("ModManager", $"Error loading mod \"{path}\" – {ex.Message}");
			}

			return null;
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
			try
			{
				string matchingFile = FindFiles(path).First();
				object reference = Bundle.LoadAsset(matchingFile, type);
				if (reference != null)
					return (reference, path, null);
			}
			catch
			{
			}
			return default;
		}

		public override async UniTask<(object reference, string filePath, ResourceParser parser)> LoadExAsync(Type type, string path)
		{
			object reference = null;
			try
			{
				await LoadLocal();
				IEnumerator LoadLocal()
				{
					string matchingFile = FindFiles(path).First();
					AssetBundleRequest request = Bundle.LoadAssetAsync(matchingFile);
					yield return request;
					reference = request.asset;
				}
			}
			catch
			{
			}

			if (reference != null)
				return (reference, path, null);

			return default;
		}

		public override string ReadText(string path)
		{
			TextAsset textAsset = (TextAsset) LoadEx(typeof(TextAsset), path).reference;
			return textAsset != null ? textAsset.text : null;
		}

		public override async UniTask<string> ReadTextAsync(string path)
		{
			TextAsset textAsset = (TextAsset) (await LoadExAsync(typeof(TextAsset), path)).reference;
			return textAsset != null ? textAsset.text : null;
		}

		public override byte[] ReadBytes(string path)
		{
			TextAsset textAsset = (TextAsset) LoadEx(typeof(TextAsset), path).reference;
			return textAsset != null ? textAsset.bytes : null;
		}

		public override async UniTask<byte[]> ReadBytesAsync(string path)
		{
			TextAsset textAsset = (TextAsset) (await LoadExAsync(typeof(TextAsset), path)).reference;
			return textAsset != null ? textAsset.bytes : null;
		}

		public override IEnumerable<string> FindFiles(string path)
		{
			path = GetAssetPath(path);
			if (Bundle.Contains(path))
				return EnumerableExtensions.One(path);

			if (Path.HasExtension(path))
				return Enumerable.Empty<string>();

			var assetNames = Bundle.GetAllAssetNames();
			return assetNames.Where(name => ResourceManager.ComparePath(path, Path.ChangeExtension(name, null)));
		}

		public override bool Exists(string path)
		{
			return Bundle.Contains(GetAssetPath(path));
		}

		// AssetBundle paths include "Assets/" and file extensions
		public static string GetAssetPath(string path)
		{
			return "Assets/" + path;
		}

		public override void Unload()
		{
			base.Unload();
			Bundle.Unload(false);
		}
	}
}
#endif