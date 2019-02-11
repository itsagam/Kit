using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

[Serializable]
public class Stat : ReactiveProperty<float>
{
	public ReadOnlyReactiveProperty<float> CurrentProperty { get; protected set; }

	protected string statName;
	protected ReactiveDictionary<string, Upgrade> upgrades;

	public void Setup(string statName, IUpgradeable upgradeable)
	{
		this.statName = statName;
		upgrades = upgradeable.Upgrades;

		var observable = upgrades.ObserveCountChanged()
			.Select(c => GetAggregates())
			.StartWith(GetAggregates())
			.CombineLatest(this, (aggregates, baseValue) => CalculateValue(aggregates, baseValue));

		CurrentProperty = new ReadOnlyReactiveProperty<float>(observable);
	}

	public float BaseValue
	{
		get
		{
			return Value;
		}
		set
		{
			Value = value;
		}
	}

	public float CurrentValue
	{
		get
		{
			return CurrentProperty.Value;
		}
	}

	protected (float, float, float) GetAggregates()
	{
		var effects = upgrades.Values.SelectMany(u => u).Where(e => e.Stat == statName);
		return GetAggregates(effects);
	}

	protected (float, float, float) GetAggregates(IEnumerable<Effect> effects)
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

	protected float CalculateValue((float value, float percent, float multiplier) aggregates, float baseValue)
	{
		return ((baseValue + aggregates.value) * aggregates.percent / 100) * aggregates.multiplier;
	}
}

// TODO: Find a way to not use strings
public class Stats : Dictionary<string, ReactiveProperty<float>>, IDisposable
{
	public ReactiveDictionary<string, Upgrade> Upgrades { get; protected set; }

	protected Dictionary<string, ReadOnlyReactiveProperty<float>> currentProperties =
		new Dictionary<string, ReadOnlyReactiveProperty<float>>();

	protected CompositeDisposable disposables = new CompositeDisposable();

	public Stats(IUpgradeable upgradeable)
	{
		Upgrades = upgradeable.Upgrades;
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

	public ReactiveProperty<float> GetBaseProperty(string stat)
	{
		if (TryGetValue(stat, out var property))
			return property;
		return null;
	}

	public float GetBaseValue(string stat)
	{
		return GetBaseProperty(stat).Value;
	}

	public void SetBaseProperty(string stat, ReactiveProperty<float> value)
	{
		base[stat] = value;
	}

	public void SetBaseValue(string stat, float value)
	{
		if (TryGetValue(stat, out var property))
			property.Value = value;
		else
		{
			var baseProperty = new ReactiveProperty<float>(value).AddTo(disposables);
			SetBaseProperty(stat, baseProperty);
		}
	}

	public ReadOnlyReactiveProperty<float> GetCurrentProperty(string stat)
	{
		if (currentProperties.TryGetValue(stat, out var property))
			return property;

		// We get the last upgrades that were changed and aggregate them, and then we use CombineLatest
		// to use these aggregates in changing base values
		var observable = Upgrades.ObserveCountChanged()
			.Select(c => GetAggregates(stat))
			.StartWith(GetAggregates(stat))
			.CombineLatest(GetBaseProperty(stat), (aggregates, baseValue) => CalculateValue(aggregates, baseValue));

		var currentProperty = new ReadOnlyReactiveProperty<float>(observable);

		disposables.Add(currentProperty);
		currentProperties.Add(stat, currentProperty);
	
		return currentProperty;
	}

	public float GetCurrentValue(string stat)
	{
		return GetCurrentProperty(stat).Value;
	}

	protected (float, float, float) GetAggregates(string stat)
	{
		var effects = Upgrades.Values.SelectMany(u => u).Where(e => e.Stat == stat);
		return GetAggregates(effects);
	}

	protected (float, float, float) GetAggregates(IEnumerable<Effect> effects)
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

	protected float CalculateValue((float value, float percent, float multiplier) aggregates, float baseValue)
	{
		return ((baseValue + aggregates.value) * aggregates.percent / 100) * aggregates.multiplier;
	}

	public void Dispose()
	{
		disposables.Dispose();
	}
}

public interface IUpgradeable
{
	ReactiveDictionary<string, Upgrade> Upgrades { get; }
}

public enum BuffMode
{
	Add,
	Extend,
	Keep,
	Replace,
	Longer,
	Shorter,
}

public class Buff : Upgrade
{
	public float Time;
	public BuffMode Mode = BuffMode.Extend;

	public Buff()
		: base()
	{
	}

	public Buff(string id, float time, IEnumerable<Effect> effects, BuffMode mode = BuffMode.Extend)
		: base(id, effects)
	{
		Time = time;
		Mode = mode;
	}

	public override void AddTo(IUpgradeable upgradeable)
	{
		AddTo(upgradeable, Mode);
	}

	public virtual void AddTo(IUpgradeable upgradeable, BuffMode mode)
	{
		if (upgradeable?.Upgrades == null)
			return;

		Buff previous = null;
		if (mode != BuffMode.Add)
		{
			upgradeable.Upgrades.TryGetValue(ID, out Upgrade upgrade);
			if (upgrade is Buff buff)
				previous = buff;
		}

		if (mode == BuffMode.Add || previous == null)
		{
			base.AddTo(upgradeable);
			Observable.Timer(TimeSpan.FromSeconds(Time)).Subscribe(l =>
			{
				try {
					base.RemoveFrom(upgradeable);
				}
				catch {}
			});
			return;
		}

		switch (mode)
		{
			case BuffMode.Keep:
				break;

			case BuffMode.Replace:
				previous.Time = Time;
				break;

			case BuffMode.Extend:
				previous.Time += Time;
				break;

			case BuffMode.Longer:
				if (previous.Time < Time)
					previous.Time = Time;
				break;

			case BuffMode.Shorter:
				if (previous.Time > Time)
					previous.Time = Time;
				break;
		}
	}
}

public class Upgrade: List<Effect>
{
	public string ID;

	public Upgrade()
	{
	}

	public Upgrade(string id, IEnumerable<Effect> effects)
	{
		ID = id;
		AddRange(effects);
	}

	public virtual void AddTo(IUpgradeable upgradeable)
	{
		upgradeable.Upgrades.Add(ID, this);
	}

	public virtual void RemoveFrom(IUpgradeable upgradeable)
	{
		upgradeable.Upgrades.Remove(ID);
	}
}

public enum EffectType
{
	Multiplier,
	Percentage,
	Value,
}

public class Effect
{
	public string Stat;
	public EffectType Type;
	public float Value;

	public Effect()
	{
	}

	public Effect(string stat, string value)
	{
		Stat = stat;
		if (value.StartsWith("x"))
		{
			Type = EffectType.Multiplier;
			Value = Convert.ToSingle(value.Substring(1));
		}
		else if (value.EndsWith("%"))
		{
			Type = EffectType.Percentage;
			Value = Convert.ToSingle(value.Substring(0, value.Length - 1));
		}
		else
		{
			Type = EffectType.Value;
			Value = Convert.ToSingle(value);
		}
	}

	public Effect(string stat, EffectType type, float value)
	{
		Stat = stat;
		Type = type;
		Value = value;
	}

	public override string ToString()
	{
		string output = Stat + ": ";

		if (Type != EffectType.Multiplier && Value > 0)
			output += "+";

		switch (Type)
		{
			case EffectType.Value:
				output += Value;
				break;

			case EffectType.Multiplier:
				output += "x" + Value;
				break;

			case EffectType.Percentage:
				output += Value + "%";
				break;
		}
		return output;
	}
}