using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using Sirenix.OdinInspector;

// TODO: Fix Reference picker showing up in empty Stat/Stats (also doesn't trigger Initialize code)

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

public class StatProcessor : OdinAttributeProcessor<Stat>
{
	public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
	{
		attributes.Add(new HideReferenceObjectPickerAttribute());
	}
}

public class StatDrawer : OdinValueDrawer<Stat>
{
	public const float FoldoutWidthCorrection = 4;
	public static GUIStyle CurrentValueStyle = new GUIStyle(SirenixGUIStyles.BoldTitle)
	{
		alignment = TextAnchor.MiddleRight,
		padding = new RectOffset(2, 6, 1, 2)
	};

	protected LocalPersistentContext<bool> toggled;

	protected override void Initialize()
	{
		base.Initialize();
		toggled = this.GetPersistentValue("Toggled", false);

		var stat = ValueEntry.SmartValue;
		if (stat != null)
		{
			if (stat.Upgradeable == null)
				stat.Upgradeable = Property.Tree.UnitySerializedObject.targetObject as IUpgradeable;

			if (stat.ID.IsNullOrEmpty())
				stat.ID = Property.Name;
		}
	}

	protected override void DrawPropertyLayout(GUIContent label)
	{
		var stat = ValueEntry.SmartValue;
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
		var currentValue = Stats.CalculateValue(Stats.GetAggregates(stat.Upgradeable, stat.ID), stat.BaseValue);
		var groups = Stats.GetEffectsAndUpgrades(stat.Upgradeable, stat.ID);
		if (groups.Any())
		{
			Rect rect = EditorGUILayout.GetControlRect(false, GUILayout.Width(EditorGUIUtility.labelWidth - FoldoutWidthCorrection));
			toggled.Value = SirenixEditorGUI.Foldout(rect, toggled.Value, label);
			DrawField(null);
		}
		else
		{
			DrawField(label);
		}

		GUIHelper.PushGUIEnabled(true);
		GUI.Label(GUILayoutUtility.GetLastRect(), Mathf.RoundToInt(currentValue).ToString(), CurrentValueStyle);
		GUIHelper.PopGUIEnabled();
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

	public void DrawField(GUIContent label)
	{
		Property.Children["Base"].Draw(label);
	}
}
#endif

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
			if (current == null)
				current = Stats.CreateCurrentProperty(Base, Upgradeable, ID);
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