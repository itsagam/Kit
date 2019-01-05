using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShowHideButton))]
public class ShowButtonEditor : Editor
{
	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		SerializedProperty property = serializedObject.GetIterator();
		property.NextVisible(true);
		ShowHideButton.ShowHideType mode = default;
		while (property.NextVisible(false))
		{
			if (property.name == "Type")
				mode = (ShowHideButton.ShowHideType) property.enumValueIndex;

			if (property.name == "Popup")
			{
				if (mode == ShowHideButton.ShowHideType.Popup)
					EditorGUILayout.PropertyField(property);
			}
			else if (property.name == "ID")
			{
				if (mode == ShowHideButton.ShowHideType.ID)
					EditorGUILayout.PropertyField(property);
			}
			else
				EditorGUILayout.PropertyField(property);
		}
		serializedObject.ApplyModifiedProperties();
	}
}