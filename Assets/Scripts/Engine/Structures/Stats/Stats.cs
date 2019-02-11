using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using Sirenix.OdinInspector;

// TODO: Find a way to not use strings
// TODO: Make serializable and code inspectors

public interface IUpgradeable
{
	ReactiveDictionary<string, Upgrade> Upgrades { get; }
}

[Serializable]
public class Stats : Dictionary<string, StatReactiveProperty>, IDisposable
{
	protected ReactiveDictionary<string, Upgrade> upgrades;

	protected Dictionary<string, ReadOnlyReactiveProperty<float>> currentProperties = new Dictionary<string, ReadOnlyReactiveProperty<float>>();
	protected CompositeDisposable disposables = new CompositeDisposable();

	public Stats(IUpgradeable upgradeable)
	{
		upgrades = upgradeable.Upgrades;
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

	public StatReactiveProperty GetBaseProperty(string stat)
	{
		if (TryGetValue(stat, out var property))
			return property;
		return null;
	}

	public void SetBaseProperty(string stat, StatReactiveProperty value)
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
			var baseProperty = new StatReactiveProperty(value).AddTo(disposables);
			SetBaseProperty(stat, baseProperty);
		}
	}

	public ReadOnlyReactiveProperty<float> GetCurrentProperty(string stat)
	{
		if (currentProperties.TryGetValue(stat, out var property))
			return property;

		var currentProperty = CreateCurrentProperty(GetBaseProperty(stat), upgrades, stat);

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
			ReactiveDictionary<string, Upgrade> upgrades,
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

	public static (float, float, float) GetAggregates(ReactiveDictionary<string, Upgrade> upgrades, string stat)
	{
		var effects = upgrades.Values.SelectMany(u => u.Effects).Where(e => e.Stat == stat);
		return GetAggregates(effects);
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