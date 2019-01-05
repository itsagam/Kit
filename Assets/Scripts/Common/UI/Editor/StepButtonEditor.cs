using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StepButton))]
public class StepButtonEditor : Editor
{
	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		SerializedProperty property = serializedObject.GetIterator();
		property.NextVisible(true);
		StepButton.StepMode mode = default;
		while (property.NextVisible(false))
		{
			if (property.name == "Mode")
				mode = (StepButton.StepMode) property.enumValueIndex;
			if (property.name != "Change" || mode == StepButton.StepMode.Change)
				EditorGUILayout.PropertyField(property);
		}
		serializedObject.ApplyModifiedProperties();
	}
}