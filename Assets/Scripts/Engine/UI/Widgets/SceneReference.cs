using System;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Engine.UI.Widgets
{
	[Serializable]
	[InlineProperty]
	public class SceneReference: ISerializationCallbackReceiver
	{
#if UNITY_EDITOR
		[PropertyOrder(-1)]
		[ShowInInspector]
		[ReadOnly]
		[HideLabel]
#endif
		public string Path { get; protected set; }

		public override string ToString()
		{
			return Path;
		}

#if UNITY_EDITOR
		[SerializeField]
		[AssetList]
		[OnValueChanged("Refresh")]
		protected SceneAsset scene;

		public void OnBeforeSerialize()
		{
			EditorDispatcher.Enqueue(Refresh);
		}

		public void OnAfterDeserialize()
		{
			EditorDispatcher.Enqueue(Refresh);
		}

		protected void Refresh()
		{
			Path = GetActualPath();
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