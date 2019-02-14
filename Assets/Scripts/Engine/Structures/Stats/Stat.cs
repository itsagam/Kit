using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

// TODO: Setup Current Value at currect time

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

		if (stat.Upgradeable != null && !stat.ID.IsNullOrEmpty())
			stat.Setup();
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
		var currentValue = Stats.CalculateValue(Stats.GetAggregates(stat.Upgradeable, stat.ID), stat.BaseValue);
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
		
		GUILayout.Label(Mathf.RoundToInt(currentValue).ToString(), CurrentValueStyle, GUILayout.ExpandWidth(false));
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

	protected void DrawField(GUIContent label, Stat stat)
	{
		Property.Children["Base"].Draw(label);
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
		Setup();
	}

	public void Setup()
	{
		if (current != null)
			return;

		current = Stats.CreateCurrentProperty(Base, Upgradeable, ID);
	}

	public ReadOnlyReactiveProperty<float> Current
	{
		get
		{
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