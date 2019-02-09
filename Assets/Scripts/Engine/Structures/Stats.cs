using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

public class Stats : Dictionary<string, float>
{
	public IUpgradeable Upgradeable { get; protected set; }

	public Stats(IUpgradeable upgradeable)
	{
		Upgradeable = upgradeable;
	}

	public new float this[string name]
	{
		get
		{
			return GetCurrentValue(name);
		}
	}

	public float GetBaseValue(string name)
	{
		return base[name];
	}

	public float GetCurrentValue(string name)
	{
		float baseValue = base[name];
		float currentValue = baseValue;
		if (Upgradeable != null && Upgradeable.Upgrades != null)
		{
			float valueSum = 0, percentSum = 0, multiplierSum = 1;
			foreach (Upgrade upgrade in Upgradeable.Upgrades)
			{
				foreach (Effect effect in upgrade)
				{
					if (effect.Stat == name)
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
				}
				currentValue += valueSum;
				currentValue += currentValue * percentSum / 100;
				currentValue *= multiplierSum;
			}
		}
		return currentValue;
	}
}

public interface IUpgradeable
{
	List<Upgrade> Upgrades { get; }
}

public class Buff : Upgrade
{
	public string ID;
	public float Time;
	public BuffMode Mode = BuffMode.Extend;

	public Buff()
	{
	}

	// TODO: Fix id resolve (Make List<Upgrade> a dictionary?)
	public Buff(string id, float time, IEnumerable<Effect> effects, BuffMode mode = BuffMode.Extend)
	{
		ID = id;
		Time = time;
		Mode = mode;
		AddRange(effects);
	}

	public void AddTo(IUpgradeable upgradeable)
	{
		AddTo(upgradeable, Mode);
	}

	public void AddTo(List<Upgrade> upgrades)
	{
		AddTo(upgrades, Mode);
	}

	public void AddTo(IUpgradeable upgradeable, BuffMode mode)
	{
		AddTo(upgradeable.Upgrades, mode);
	}

	public void AddTo(List<Upgrade> upgrades, BuffMode mode)
	{
		if (upgrades == null)
			return;

		Buff previous = null;
		if (mode != BuffMode.Add)
			previous = upgrades.OfType<Buff>().FirstOrDefault(b => b.ID == ID);

		if (mode == BuffMode.Add || previous == null)
		{
			upgrades.Add(this);
			Observable.Timer(TimeSpan.FromSeconds(Time)).Subscribe(l =>
			{
				try {
					upgrades.Remove(this);
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

public enum BuffMode
{
	Add,
	Extend,
	Keep,
	Replace,
	Longer,
	Shorter,
}

public class Upgrade: List<Effect>
{
}

public class Effect
{
	// TODO: Find a way to not use strings
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
		switch (Type)
		{
			case EffectType.Value:
				if (Value > 0)
					output += "+";
				output += Value;
				break;

			case EffectType.Multiplier:
				output += "x" + Value;
				break;

			case EffectType.Percentage:
				if (Value > 0)
					output += "+";
				output += Value + "%";
				break;
		}
		return output;
	}
}

public enum EffectType
{
	Multiplier,
	Percentage,
	Value,
}