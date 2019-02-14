using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

public class StatsProcessor : OdinAttributeProcessor<Stats>
{
	public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
	{
		attributes.Add(new HideReferenceObjectPickerAttribute());
	}
}

public class StatsDrawer : OdinValueDrawer<Stats>
{
	public const float FoldoutGap = 15;
	public static GUIStyle BaseValueStyle = new GUIStyle(SirenixGUIStyles.Label);
	public static GUIStyle CurrentValueStyle = new GUIStyle(SirenixGUIStyles.BoldTitle)
	{
		alignment = TextAnchor.MiddleRight
	};
	public static GUIStyle EffectsStyle = new GUIStyle(SirenixGUIStyles.Label)
	{
	};

	protected LocalPersistentContext<bool> toggled;

	protected override void Initialize()
	{
		base.Initialize();
		toggled = this.GetPersistentValue("Toggled", false);
		Property.Info.GetEditableAttributesList().Add(new DictionaryDrawerSettings
		{
			KeyLabel = "Stat Name",
			ValueLabel = "Base Value"
		});
		var stats = ValueEntry.SmartValue;
		if (stats != null && stats.Upgradeable == null)
			stats.Upgradeable = Property.Tree.UnitySerializedObject.targetObject as IUpgradeable;
	}

	protected override void DrawPropertyLayout(GUIContent label)
	{
		CallNextDrawer(label);

		var stats = ValueEntry.SmartValue;
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
				var stat = kvp.Key;
				var baseValue = kvp.Value.Value;
				var currentValue = Stats.CalculateValue(Stats.GetAggregates(stats.Upgradeable, stat), baseValue);
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
						EditorGUILayout.LabelField("Base", baseValue.ToString(), BaseValueStyle);
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
			SirenixEditorGUI.WarningMessageBox($"The parent type {property.ParentType} doesn't implement IUpgradeable. Current values will not be available.");
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
		foreach (var (upgrade, effects) in groups)
		{
			string effectString = effects.Select(e => Effect.Convert(e)).Join();
			EditorGUILayout.LabelField(upgrade.ID, effectString, EffectsStyle);
		}
	}
}

public class StatBasePropertyProcessor : OdinAttributeProcessor<StatBaseProperty>
{
	public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
	{
		attributes.Add(new HideReferenceObjectPickerAttribute());
	}
}

public class StatBasePropertyDrawer : OdinValueDrawer<StatBaseProperty>
{
	protected override void DrawPropertyLayout(GUIContent label)
	{
		var stat = ValueEntry.SmartValue;
		stat.Value = SirenixEditorGUI.DynamicPrimitiveField(label, stat.Value);
	}
}
#endif

public class Stats : Dictionary<string, StatBaseProperty>, IDisposable
{
	public IUpgradeable Upgradeable;

	protected Dictionary<string, ReadOnlyReactiveProperty<float>> currentProperties = new Dictionary<string, ReadOnlyReactiveProperty<float>>();
	protected CompositeDisposable disposables = new CompositeDisposable();

	public Stats()
	{
	}

	public Stats(IUpgradeable upgradeable)
	{
		Upgradeable = upgradeable;
	}

	public new float this[string stat]
	{
		get
		{
			return GetCurrentValue(stat);
		}
		set
		{
			SetBaseValue(stat, value);
		}
	}

	public void Add(string stat, float value)
	{
		SetBaseValue(stat, value);
	}

	public StatBaseProperty GetBaseProperty(string stat)
	{
		if (TryGetValue(stat, out var property))
			return property;
		return null;
	}

	public void SetBaseProperty(string stat, StatBaseProperty value)
	{
		base[stat] = value;
	}

	public float GetBaseValue(string stat)
	{
		return GetBaseProperty(stat).Value;
	}

	public void SetBaseValue(string stat, float value)
	{
		if (TryGetValue(stat, out var property))
			property.Value = value;
		else
		{
			var baseProperty = new StatBaseProperty(value).AddTo(disposables);
			SetBaseProperty(stat, baseProperty);
		}
	}

	public ReadOnlyReactiveProperty<float> GetCurrentProperty(string stat)
	{
		if (currentProperties.TryGetValue(stat, out var property))
			return property;

		var currentProperty = CreateCurrentProperty(GetBaseProperty(stat), Upgradeable, stat);

		disposables.Add(currentProperty);
		currentProperties.Add(stat, currentProperty);
	
		return currentProperty;
	}

	public float GetCurrentValue(string stat)
	{
		return GetCurrentProperty(stat).Value;
	}

	public void Dispose()
	{
		disposables.Dispose();
	}

	public static ReadOnlyReactiveProperty<float> CreateCurrentProperty(
			ReactiveProperty<float> baseProperty,
			IUpgradeable upgradeable,
			string stat)
	{
		// We get the last upgrades that were changed and aggregate them, and then we use CombineLatest
		// to use these aggregates in changing base values
		var observable = upgradeable.GetUpgrades().ObserveCountChanged()
			.Select(c => GetAggregates(upgradeable, stat))
			.StartWith(GetAggregates(upgradeable, stat))
			.CombineLatest(baseProperty, (aggregates, baseValue) => CalculateValue(aggregates, baseValue));

		return new ReadOnlyReactiveProperty<float>(observable);
	}

	public static IEnumerable<(Upgrade, IEnumerable<Effect>)> GetEffectsAndUpgrades(IUpgradeable upgradeable, string stat)
	{
		return upgradeable.GetUpgrades()
			.Where(u => u != null)
			.Select(u => (upgrade: u, effects: u.Effects.Where(e => e.Stat == stat)))
			.Where(g => g.effects.Any());
	}

	public static IEnumerable<Effect> GetEffects(IUpgradeable upgradeable, string stat)
	{
		return upgradeable.GetUpgrades().Where(u => u != null).SelectMany(u => u.Effects).Where(e => e.Stat == stat);
	}

	public static (float, float, float) GetAggregates(IUpgradeable upgradeable, string stat)
	{
		return GetAggregates(GetEffects(upgradeable, stat));
	}

	public static (float, float, float) GetAggregates(IEnumerable<Effect> effects)
	{
		float valueSum = 0, percentSum = 100, multiplierSum = 1;
		foreach (var effect in effects)
		{
			switch (effect.Type)
			{
				case EffectType.Value:
					valueSum += effect.Value;
					break;

				case EffectType.Percentage:
					percentSum += effect.Value;
					break;

				case EffectType.Multiplier:
					multiplierSum *= effect.Value;
					break;
			}
		}
		return (valueSum, percentSum, multiplierSum);
	}

	public static float CalculateValue((float value, float percent, float multiplier) aggregates, float baseValue)
	{
		return ((baseValue + aggregates.value) * aggregates.percent / 100) * aggregates.multiplier;
	}
}

[Serializable]
public class StatBaseProperty : ReactiveProperty<float>
{
	public StatBaseProperty() : base() { }
	public StatBaseProperty(float initialValue) : base(initialValue) { }
}