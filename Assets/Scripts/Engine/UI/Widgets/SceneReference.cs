using System;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
#endif

namespace Engine.UI.Widgets
{
	[Serializable]
	[InlineProperty]
	public class SceneReference: ISerializationCallbackReceiver
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

#if UNITY_EDITOR
		[SerializeField]
		[AssetList]
		[OnValueChanged("Refresh")]
		protected SceneAsset scene;

		public void OnBeforeSerialize()
		{
			UnityEditorEventUtility.DelayAction(Refresh);
		}

		public void OnAfterDeserialize()
		{
			UnityEditorEventUtility.DelayAction(Refresh);
		}

		protected void Refresh()
		{
			path = GetActualPath();
		}

		private string GetActualPath()
		{
			return scene == null ? string.Empty : AssetDatabase.GetAssetPath(scene);
		}

		// private SceneAsset GetActualScene()
		// {
		// 	return string.IsNullOrEmpty(Path) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(Path);
		// }
#endif
	}
}