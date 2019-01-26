using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

[CanEditMultipleObjects, CustomEditor(typeof(RaycastGraphic), false)]
public class RaycastGraphicEditor : GraphicEditor
{
	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		EditorGUILayout.PropertyField(m_Script, new GUILayoutOption[0]);
		// skipping AppearanceControlsGUI
		RaycastControlsGUI();
		serializedObject.ApplyModifiedProperties();
	}
}