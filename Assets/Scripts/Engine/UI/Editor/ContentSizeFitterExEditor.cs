using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

[CanEditMultipleObjects, CustomEditor(typeof(ContentSizeFitterEx), false)]
public class ContentSizeFitterExEditor : ContentSizeFitterEditor
{
	protected SerializedProperty padding;

	protected override void OnEnable()
	{
		base.OnEnable();
		padding = serializedObject.FindProperty("Padding");
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		serializedObject.Update();
		EditorGUILayout.PropertyField(padding, true, new GUILayoutOption[0]);
		serializedObject.ApplyModifiedProperties();
	}
}