using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
#endif

namespace Engine.UI.Widgets
{
	[Serializable]
	public class WindowReference: SoftReference<Window>
	{
	}

	[Serializable]
	public class SceneReference: SoftReference<SceneAsset>
	{
#if UNITY_EDITOR
		// Scenes do not need to be under a "Resources" folder for them to be loaded using their paths
		// so we're turning off trimming and validation for them

		protected override bool Validate(SceneAsset asset)
		{
			return true;
		}

		protected override void Refresh()
		{
			path = Asset != null ? GetAssetPath() : string.Empty;
		}
#endif
	}

	[Serializable]
	[InlineProperty]
	public class SoftReference<T> : ISerializationCallbackReceiver where T: Object
	{
		[SerializeField]
		[ReadOnly]
		[HideLabel]
		protected string path;
		public string Path => path;

		public override string ToString()
		{
			return path;
		}

		public static implicit operator string(SoftReference<T> reference)
		{
			return reference.Path;
		}

#if UNITY_EDITOR
		public const string ResourcesFolder = "Resources/";

		[SerializeField]
		[AssetList]
		[AssetsOnly]
		[ValidateInput("Validate", "The asset must be under a \"Resources\" folder for it be to be loaded at runtime.",
					ContiniousValidationCheck = true)]
		[OnValueChanged("Refresh")]
		public T Asset;

		public void OnBeforeSerialize()
		{
			UnityEditorEventUtility.DelayAction(Refresh);
		}

		public void OnAfterDeserialize()
		{
			UnityEditorEventUtility.DelayAction(Refresh);
		}

		protected virtual bool Validate(T asset)
		{
			return asset == null || AssetDatabase.GetAssetPath(asset).Contains(ResourcesFolder);
		}

		protected virtual void Refresh()
		{
			path = Asset != null ? TrimResources(GetAssetPath()) : string.Empty;
		}

		protected string TrimResources(string fullPath)
		{
			int resourcesIndex = fullPath.IndexOf(ResourcesFolder, StringComparison.Ordinal);
			return resourcesIndex > 0 ? fullPath.Substring(resourcesIndex + ResourcesFolder.Length) : fullPath;
		}

		protected string GetAssetPath()
		{
			return AssetDatabase.GetAssetPath(Asset);
		}
#endif
	}
}