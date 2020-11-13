using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
#endif

namespace Engine.UI
{
	/// <summary>
	/// A <see cref="SoftReference{T}"/> (path-only) to a <see cref="Window"/>. Here because Unity can be picky about serializing generic
	/// variables.
	/// </summary>
	[Serializable]
	public class WindowReference: SoftReference<Window>
	{
	}

	/// <summary>
	/// A <see cref="SoftReference{T}"/> (path) to a scene.
	/// </summary>
	[Serializable]
#if UNITY_EDITOR
	public class SceneReference: SoftReference<SceneAsset>
#else
	public class SceneReference: SoftReference<Object>
#endif
	{
#if UNITY_EDITOR
		// Scenes do not need to be under a "Resources" folder for them to be loaded using their paths,
		// so we're turning off validation for them
		protected override bool OnValidate(SceneAsset asset)
		{
			return true;
		}
#endif
	}

	/// <summary>
	/// A class that allows one to select assets in the inspector without hard-referencing them. It saves their path instead which can
	/// later be loaded with <see cref="Load"/>.
	/// </summary>
	/// <remarks>Can be used directly as a string without needing to call <see cref="ToString"/>.</remarks>
	/// <typeparam name="T">Type of the unity object. Used to filter assets.</typeparam>
	[Serializable]
	[InlineProperty]
	public class SoftReference<T> where T: Object
	{
		public const string ResourcesFolder = "/Resources/";

		[HideLabel]
		[ReadOnly]
		[SerializeField]
		protected string fullPath;

		public string Path => TrimPath(fullPath);
		public string FullPath => fullPath;

		public T Load()
		{
			return ResourceManager.Load<T>(ResourceFolder.Resources, fullPath);
		}

		public override string ToString()
		{
			return Path;
		}

		public static implicit operator string(SoftReference<T> reference)
		{
			return reference.Path;
		}

		private string TrimPath(string inputPath)
		{
			int resourcesIndex = inputPath.IndexOf(ResourcesFolder, StringComparison.Ordinal);
			return resourcesIndex > 0 ? inputPath.Substring(resourcesIndex + ResourcesFolder.Length) : inputPath;
		}

#if UNITY_EDITOR
		[ShowInInspector]
		[HideLabel]
		[AssetsOnly, AssetList]
		[OnInspectorGUI("OnDraw")]
		[OnValueChanged("OnChanged")]
		[ValidateInput("OnValidate", "The asset must be under a \"Resources\" folder for it be to be loaded at runtime.")]
		protected T asset;

		protected virtual void OnDraw()
		{
			if (asset == null && fullPath != string.Empty)
				UnityEditorEventUtility.DelayAction(() => asset = LoadAsset_Editor());
		}

		protected virtual void OnChanged()
		{
			SetAsset_Editor(asset);
		}

		protected virtual bool OnValidate(T newAsset)
		{
			return newAsset == null || AssetDatabase.GetAssetPath(newAsset).Contains(ResourcesFolder);
		}

		public virtual void SetAsset_Editor(T newAsset)
		{
			asset = newAsset;
			fullPath = newAsset != null ? AssetDatabase.GetAssetPath(newAsset) : string.Empty;
		}

		public virtual T LoadAsset_Editor()
		{
			return AssetDatabase.LoadAssetAtPath<T>(fullPath);
		}
#endif
	}
}