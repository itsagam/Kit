using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

// TODO: Display current value and applied upgrades in Stat and Stats
// TODO: Make UpgradeList drawer
// TODO: Find a way to not use strings
// TODO: Display remaining time in Buff

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

public class StatsDrawer : OdinValueDrawer<Stats>
{
	public static GUIStyle BaseValueStyle;
	public static GUIStyle CurrentValueStyle;
	public static GUIStyle EffectsStyle;

	protected LocalPersistentContext<bool> toggled;

	protected override void Initialize()
	{
		base.Initialize();
		CurrentValueStyle = new GUIStyle(EditorStyles.whiteLabel)
		{
			alignment = TextAnchor.MiddleRight
		};
		BaseValueStyle = new GUIStyle(EditorStyles.label)
		{
		};
		EffectsStyle = new GUIStyle(EditorStyles.label)
		{
		};
		toggled = this.GetPersistentValue("Toggled", false);
		Property.Info.GetEditableAttributesList().Add(new DictionaryDrawerSettings
		{
			KeyLabel = "Stat Name",
			ValueLabel = "Base Value"
		});
		var stats = ValueEntry.SmartValue;
		if (stats.Upgradeable == null)
			stats.Upgradeable = Property.Tree.UnitySerializedObject.targetObject as IUpgradeable;
	}

	protected override void DrawPropertyLayout(GUIContent label)
	{
		CallNextDrawer(label);

		var stats = ValueEntry.SmartValue;
		if (stats.Upgradeable == null)
		{
			SirenixEditorGUI.WarningMessageBox($"The parent type {Property.ParentType} doesn't implement IUpgradeable. Current values cannot be displayed.");
			return;
		}

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
				float baseValue = kvp.Value.Value;
				float currentValue = stats.GetCurrentValue(stat);

				var groups = stats.Upgradeable.GetUpgrades()
								.Select(u => (upgrade: u, effects: u.Effects.Where(e => e.Stat == stat)))
								.Where(g => g.effects.Any());

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
					GUILayout.Space(15);
					GUILayout.Label(stat);
				}
				GUILayout.Label(currentValue.ToString(), CurrentValueStyle);

				SirenixEditorGUI.EndIndentedHorizontal();
				SirenixEditorGUI.EndBoxHeader();
		
				if (isExpanded != null)
				{
					// Upgrades and effects
					if (SirenixEditorGUI.BeginFadeGroup(isExpanded, isExpanded.Value))
					{
						GUIHelper.PushIndentLevel(1);
						SirenixEditorGUI.BeginHorizontalPropertyLayout(new GUIContent("Base"));
						GUILayout.Label(baseValue.ToString(), BaseValueStyle);
						SirenixEditorGUI.EndHorizontalPropertyLayout();
						
						SirenixEditorGUI.DrawThickHorizontalSeparator(2, 2);

						foreach (var (upgrade, effects) in groups)
						{
							string effectString = effects.Select(e => Effect.Convert(e)).Join();
							SirenixEditorGUI.BeginHorizontalPropertyLayout(new GUIContent(upgrade.ID));
							GUILayout.Label(effectString, EffectsStyle);
							SirenixEditorGUI.EndHorizontalPropertyLayout();
						}
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
		Property.Children[0].Draw(label);
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

		var currentProperty = CreateCurrentProperty(GetBaseProperty(stat), Upgradeable.GetUpgrades(), stat);

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
			ReactiveCollection<Upgrade> upgrades,
			string stat)
	{
		// We get the last upgrades that were changed and aggregate them, and then we use CombineLatest
		// to use these aggregates in changing base values
		var observable = upgrades.ObserveCountChanged()
			.Select(c => GetAggregates(upgrades, stat))
			.StartWith(GetAggregates(upgrades, stat))
			.CombineLatest(baseProperty, (aggregates, baseValue) => CalculateValue(aggregates, baseValue));

		return new ReadOnlyReactiveProperty<float>(observable);
	}

	public static IEnumerable<Effect> GetMatchingEffects(ReactiveCollection<Upgrade> upgrades, string stat)
	{
		return upgrades.SelectMany(u => u.Effects).Where(e => e.Stat == stat);
	}

	public static (float, float, float) GetAggregates(ReactiveCollection<Upgrade> upgrades, string stat)
	{
		return GetAggregates(GetMatchingEffects(upgrades, stat));
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