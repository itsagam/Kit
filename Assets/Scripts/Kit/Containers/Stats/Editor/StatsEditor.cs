using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Kit.Containers.Editor
{
	public class StatsProcessor: OdinAttributeProcessor<Stats>
	{
		public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
		{
			attributes.Add(new HideReferenceObjectPickerAttribute());
			attributes.Add(new DictionaryDrawerSettings
						   {
							   KeyLabel = "Stat Name",
							   ValueLabel = "Base Value"
						   });
		}
	}

	public class StatsDrawer: OdinValueDrawer<Stats>
	{
		public const float FoldoutGap = 15;
		public static readonly GUIStyle BaseValueStyle = new GUIStyle(SirenixGUIStyles.Label);

		public static readonly GUIStyle CurrentValueStyle = new GUIStyle(SirenixGUIStyles.BoldTitle)
															{
																alignment = TextAnchor.MiddleRight
															};

		public static readonly GUIStyle EffectsStyle = new GUIStyle(SirenixGUIStyles.Label);

		protected LocalPersistentContext<bool> toggled;

		protected override void Initialize()
		{
			base.Initialize();
			toggled = this.GetPersistentValue("Toggled", false);
			SetupInstance();
		}

		protected void SetupInstance()
		{
			// If a value is reference type and isn't assigned, Odin doesn't create a new instance like Unity and we have
			// to pick it manually every time. Setting it immediately messes up rendering, so we delay it until that.
			if (ValueEntry.SmartValue == null)
				Property.Tree.DelayActionUntilRepaint(() => ValueEntry.SmartValue = new Stats());
			// Try to setup values required by the instance
			SetupValues();
			// We have to setup the instance whenever it's changed/set in the inspector
			ValueEntry.OnValueChanged += i => SetupValues();
		}

		protected void SetupValues()
		{
			Stats stats = ValueEntry.SmartValue;
			if (stats != null && stats.Upgradeable == null)
				stats.Upgradeable = Property.Tree.UnitySerializedObject.targetObject as IUpgradeable;
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			CallNextDrawer(label);

			Stats stats = ValueEntry.SmartValue;
			if (DrawWarning(Property, stats.Upgradeable))
				return;

			// Current header
			SirenixEditorGUI.BeginIndentedVertical(SirenixGUIStyles.PropertyPadding);
			SirenixEditorGUI.BeginHorizontalToolbar();
			toggled.Value = SirenixEditorGUI.Foldout(toggled.Value, "Current");
			SirenixEditorGUI.EndHorizontalToolbar();

			if (SirenixEditorGUI.BeginFadeGroup(UniqueDrawerKey.Create(Property, this), toggled.Value))
			{
				int i = 0;
				foreach (var kvp in stats)
				{
					string stat = kvp.Key;
					float baseValue = kvp.Value.Value;
					float currentValue = Stats.CalculateValue(stats.Upgradeable, stat, baseValue);
					var groups = Stats.GetEffectsAndUpgrades(stats.Upgradeable, stat);

					// Stat header
					SirenixEditorGUI.BeginBox();
					SirenixEditorGUI.BeginBoxHeader();
					SirenixEditorGUI.BeginIndentedHorizontal();
					LocalPersistentContext<bool> isExpanded = null;
					if (groups.Any())
					{
						isExpanded = Property.Children[i].Context.GetPersistent(this, "CurrentExpanded", false);
						isExpanded.Value = SirenixEditorGUI.Foldout(isExpanded.Value, stat);
					}
					else
					{
						GUILayout.Space(FoldoutGap);
						GUILayout.Label(stat);
					}

					GUILayout.Label(Mathf.RoundToInt(currentValue).ToString(), CurrentValueStyle);

					SirenixEditorGUI.EndIndentedHorizontal();
					SirenixEditorGUI.EndBoxHeader();

					if (isExpanded != null)
					{
						// Upgrades and effects
						if (SirenixEditorGUI.BeginFadeGroup(isExpanded, isExpanded.Value))
						{
							GUIHelper.PushIndentLevel(1);
							EditorGUILayout.LabelField("Base", baseValue.ToString(CultureInfo.CurrentCulture), BaseValueStyle);
							SirenixEditorGUI.DrawThickHorizontalSeparator(1, 1);
							DrawEffects(groups);
							GUIHelper.PopIndentLevel();
						}

						SirenixEditorGUI.EndFadeGroup();
					}

					SirenixEditorGUI.EndBox();
					i++;
				}
			}

			SirenixEditorGUI.EndFadeGroup();
			SirenixEditorGUI.EndIndentedVertical();
		}

		public static bool DrawWarning(InspectorProperty property, IUpgradeable upgradeable)
		{
			if (upgradeable == null)
			{
				SirenixEditorGUI
				   .WarningMessageBox($"The parent type {property.ParentType} doesn't implement IUpgradeable. Current values will not be available.");
				return true;
			}

			if (upgradeable.GetUpgrades() == null)
			{
				SirenixEditorGUI.WarningMessageBox("Trying to fetch upgrades returned null. Current values will not be available.");
				return true;
			}

			return false;
		}

		public static void DrawEffects(IEnumerable<(Upgrade, IEnumerable<Effect>)> groups)
		{
			foreach ((Upgrade upgrade, var effects) in groups)
			{
				string effectString = effects.Select(Effect.Convert).Join();
				EditorGUILayout.LabelField(upgrade.ID, effectString, EffectsStyle);
			}
		}
	}

	public class StatBasePropertyProcessor: OdinAttributeProcessor<StatBaseProperty>
	{
		public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
		{
			attributes.Add(new HideReferenceObjectPickerAttribute());
		}
	}

	public class StatBasePropertyDrawer: OdinValueDrawer<StatBaseProperty>
	{
		protected override void DrawPropertyLayout(GUIContent label)
		{
			StatBaseProperty stat = ValueEntry.SmartValue;
			stat.Value = SirenixEditorGUI.DynamicPrimitiveField(label, stat.Value);
		}
	}
}