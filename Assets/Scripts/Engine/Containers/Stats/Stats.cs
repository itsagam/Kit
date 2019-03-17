using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Engine.Containers
{
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
			get => GetCurrentValue(stat);
			set => SetBaseValue(stat, value);
		}

		public void Add(string stat, float value)
		{
			SetBaseValue(stat, value);
		}

		public StatBaseProperty GetBaseProperty(string stat)
		{
			return this.GetOrDefault(stat);
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
			if (TryGetValue(stat, out StatBaseProperty property))
				property.Value = value;
			else
			{
				StatBaseProperty baseProperty = new StatBaseProperty(value).AddTo(disposables);
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

		public static ReadOnlyReactiveProperty<float> CreateCurrentProperty(ReactiveProperty<float> baseProperty,
																			IUpgradeable upgradeable,
																			string stat)
		{
			// We get the last upgrades that were changed and aggregate them, and then we use CombineLatest
			// to use these aggregates in changing base values
			var observable = upgradeable.GetUpgrades()
										.ObserveCountChanged()
										.Select(c => GetAggregates(upgradeable, stat))
										.StartWith(GetAggregates(upgradeable, stat))
										.CombineLatest(baseProperty, CalculateValue);

			return new ReadOnlyReactiveProperty<float>(observable);
		}

		public static (float, float, float) GetAggregates(IUpgradeable upgradeable, string stat)
		{
			return GetAggregates(GetEffects(upgradeable, stat));
		}

		public static float CalculateValue(IUpgradeable upgradeable, string stat, float baseValue)
		{
			return CalculateValue(GetAggregates(upgradeable, stat), baseValue);
		}

		public static IEnumerable<(Upgrade, IEnumerable<Effect>)> GetEffectsAndUpgrades(IUpgradeable upgradeable, string stat)
		{
			return upgradeable.GetUpgrades()
							  .Select(u => (upgrade: u, effects: u.Effects.Where(e => e.Stat == stat)))
							  .Where(g => g.effects.Any());
		}

		public static IEnumerable<Effect> GetEffects(IUpgradeable upgradeable, string stat)
		{
			return upgradeable.GetUpgrades().Where(u => u != null).SelectMany(u => u.Effects).Where(e => e.Stat == stat);
		}

		public static (float, float, float) GetAggregates(IEnumerable<Effect> effects)
		{
			float valueSum = 0, percentSum = 100, multiplierSum = 1;
			foreach (Effect effect in effects)
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
			return (baseValue + aggregates.value) * aggregates.percent / 100 * aggregates.multiplier;
		}
	}

	[Serializable]
	public class StatBaseProperty : ReactiveProperty<float>
	{
		public StatBaseProperty() { }

		public StatBaseProperty(float initialValue) : base(initialValue) { }
	}
}