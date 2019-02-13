using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

// TODO: Check if current values are updated in the editor

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

public class StatDrawer : OdinValueDrawer<Stat>
{
	public const float FoldoutWidthCorrection = 4;
	public static GUIStyle CurrentValueStyle = new GUIStyle(SirenixGUIStyles.BoldTitle);

	protected LocalPersistentContext<bool> toggled;

	protected override void Initialize()
	{
		base.Initialize();
		toggled = this.GetPersistentValue("Toggled", false);

		var stat = ValueEntry.SmartValue;
		if (stat.Upgradeable == null)
			stat.Upgradeable = Property.Tree.UnitySerializedObject.targetObject as IUpgradeable;

		if (stat.ID.IsNullOrEmpty())
			stat.ID = Property.Name;
	}

	protected override void DrawPropertyLayout(GUIContent label)
	{
		var stat = ValueEntry.SmartValue;
		if (StatsDrawer.DrawWarning(Property, stat.Upgradeable))
		{
			DrawField(label, stat);
			return;
		}

		if (stat.ID.IsNullOrEmpty())
		{
			DrawField(label, stat);
			SirenixEditorGUI.WarningMessageBox("Could not set stat ID. Current values will not be available.");
			return;
		}

		SirenixEditorGUI.BeginIndentedHorizontal();
		var groups = Stats.GetEffectsAndUpgrades(stat.Upgradeable, stat.ID);
		if (groups.Any())
		{
			Rect rect = EditorGUILayout.GetControlRect(false, GUILayout.Width(EditorGUIUtility.labelWidth - FoldoutWidthCorrection));
			toggled.Value = SirenixEditorGUI.Foldout(rect, toggled.Value, label);
			DrawField(null, stat);
		}
		else
		{
			DrawField(label, stat);
		}

		GUILayout.Label(Mathf.RoundToInt(stat.CurrentValue).ToString(), CurrentValueStyle, GUILayout.ExpandWidth(false));
		SirenixEditorGUI.EndIndentedHorizontal();

		if (groups.Any())
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

	public static void DrawField(GUIContent label, Stat stat)
	{
		stat.BaseValue = SirenixEditorGUI.DynamicPrimitiveField(label, stat.BaseValue);
	}
}
#endif

[Serializable]
public class Stat : IDisposable
{
	public IUpgradeable Upgradeable;
	public string ID;

	public StatBaseProperty Base = new StatBaseProperty();
	protected ReadOnlyReactiveProperty<float> current;

	public Stat()
	{
	}

	public Stat(IUpgradeable upgradeable, string id)
	{
		Upgradeable = upgradeable;
		ID = id;
	}

	public ReadOnlyReactiveProperty<float> Current
	{
		get
		{
			if (current == null
				&& Upgradeable?.GetUpgrades() != null
				&& !ID.IsNullOrEmpty())
			{
				current = Stats.CreateCurrentProperty(Base, Upgradeable, ID);
			}
			return current;
		}
	}

	public float BaseValue
	{
		get
		{
			return Base.Value;
		}
		set
		{
			Base.Value = value;
		}
	}

	public float CurrentValue
	{
		get
		{
			return Current.Value;
		}
	}

	public void Dispose()
	{
		Base.Dispose();
		Current?.Dispose();
	}
}