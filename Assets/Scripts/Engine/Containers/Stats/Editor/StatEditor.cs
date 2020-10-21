using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Engine.Containers.Editor
{
	public class StatProcessor: OdinAttributeProcessor<Stat>
	{
		public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
		{
			attributes.Add(new HideReferenceObjectPickerAttribute());
		}
	}

	public class StatDrawer: OdinValueDrawer<Stat>
	{
		public const float FoldoutWidthCorrection = 4;

		public static readonly GUIStyle CurrentValueStyle = new GUIStyle(SirenixGUIStyles.BoldTitle)
															{
																alignment = TextAnchor.MiddleRight,
																padding = new RectOffset(2, 6, 1, 2)
															};

		protected LocalPersistentContext<bool> toggled;

		protected override void Initialize()
		{
			base.Initialize();
			toggled = this.GetPersistentValue("Toggled", false);
			SetupInstance();
		}

		protected void SetupInstance()
		{
			if (ValueEntry.SmartValue == null)
				Property.Tree.DelayActionUntilRepaint(() => ValueEntry.SmartValue = new Stat());
			SetupValues();
			ValueEntry.OnValueChanged += i => SetupValues();
		}

		protected void SetupValues()
		{
			Stat stat = ValueEntry.SmartValue;
			if (stat == null)
				return;

			if (stat.Upgradeable == null)
				stat.Upgradeable = Property.Tree.UnitySerializedObject.targetObject as IUpgradeable;

			if (stat.ID.IsNullOrEmpty())
				stat.ID = Property.Name;
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			Stat stat = ValueEntry.SmartValue;
			if (stat.Upgradeable?.GetUpgrades() == null)
			{
				DrawField(label);
				StatsDrawer.DrawWarning(Property, stat.Upgradeable);
				return;
			}

			if (stat.ID.IsNullOrEmpty())
			{
				DrawField(label);
				SirenixEditorGUI.WarningMessageBox("Could not set stat ID. Current values will not be available.");
				return;
			}

			SirenixEditorGUI.BeginIndentedHorizontal();
			var groups = Stats.GetEffectsAndUpgrades(stat.Upgradeable, stat.ID);
			bool any = groups.Any();
			if (any)
			{
				Rect rect = EditorGUILayout.GetControlRect(false, GUILayout.Width(EditorGUIUtility.labelWidth - FoldoutWidthCorrection));
				toggled.Value = SirenixEditorGUI.Foldout(rect, toggled.Value, label);
				DrawField(null);
			}
			else
				DrawField(label);

			float currentValue = Stats.CalculateValue(stat.Upgradeable, stat.ID, stat.BaseValue);
			GUI.Label(GUILayoutUtility.GetLastRect(), Mathf.RoundToInt(currentValue).ToString(), CurrentValueStyle);
			SirenixEditorGUI.EndIndentedHorizontal();
			if (any)
			{
				if (SirenixEditorGUI.BeginFadeGroup(UniqueDrawerKey.Create(Property, this), toggled.Value))
				{
					GUIHelper.PushIndentLevel(1);
					StatsDrawer.DrawEffects(groups);
					GUIHelper.PopIndentLevel();
				}

				SirenixEditorGUI.EndFadeGroup();
			}
		}

		public void DrawField(GUIContent label)
		{
			Property.Children["Base"].Draw(label);
		}
	}
}