using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Kit.Containers.Editor
{
	public class BuffProcessor: OdinAttributeProcessor<Buff>
	{
		public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
		{
			switch (member.Name)
			{
				case "Duration":
					attributes.Add(new HideInPlayModeAttribute());
					attributes.Add(new HorizontalGroupAttribute());
					attributes.Add(new SuffixLabelAttribute("sec", true));
					break;

				case "Mode":
					attributes.Add(new HideInPlayModeAttribute());
					attributes.Add(new HorizontalGroupAttribute(90));
					attributes.Add(new HideLabelAttribute());
					break;

				case "Effects":
					attributes.Add(new PropertyOrderAttribute(99));
					break;
			}
		}
	}

	public class BuffDrawer: OdinValueDrawer<Buff>
	{
		protected override void DrawPropertyLayout(GUIContent label)
		{
			EditorUtility.SetDirty(Property.Tree.UnitySerializedObject.targetObject);

			Buff buff = ValueEntry.SmartValue;
			SirenixEditorGUI.BeginToolbarBox(label);

			if (Application.isPlaying && buff.TimeLeft > 0)
			{
				GUIHelper.PushGUIEnabled(false);
				EditorGUILayout.LabelField("Time", $"{buff.TimeLeft:0.##}s");
				GUIHelper.PopGUIEnabled();
			}

			CallNextDrawer(null);
			SirenixEditorGUI.EndToolbarBox();
		}
	}
}