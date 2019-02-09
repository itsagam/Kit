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
		set
		{
			SetBaseValue(name, value);
		}
	}

	public float GetBaseValue(string name)
	{
		return base[name];
	}

	public float SetBaseValue(string name, float value)
	{
		return base[name] = value;
	}

	// TODO: Cache values rather than calculate every time
	public float GetCurrentValue(string name)
	{
		float baseValue = base[name];
		float currentValue = baseValue;
		if (Upgradeable != null && Upgradeable.Upgrades != null)
		{
			float valueSum = 0, percentSum = 0, multiplierSum = 1;
			foreach (Upgrade upgrade in Upgradeable.Upgrades.Values)
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
	Dictionary<string, Upgrade> Upgrades { get; }
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

	public override void AddTo(Dictionary<string, Upgrade> upgrades)
	{
		AddTo(upgrades, Mode);
	}

	public virtual void AddTo(IUpgradeable upgradeable, BuffMode mode)
	{
		AddTo(upgradeable.Upgrades, mode);
	}

	public virtual void AddTo(Dictionary<string, Upgrade> upgrades, BuffMode mode)
	{
		if (upgrades == null)
			return;

		Buff previous = null;
		if (mode != BuffMode.Add)
		{
			upgrades.TryGetValue(ID, out Upgrade upgrade);
			if (upgrade is Buff buff)
				previous = buff;
		}

		if (mode == BuffMode.Add || previous == null)
		{
			base.AddTo(upgrades);
			Observable.Timer(TimeSpan.FromSeconds(Time)).Subscribe(l =>
			{
				try {
					base.RemoveFrom(upgrades);
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
		AddTo(upgradeable.Upgrades);
	}

	public virtual void AddTo(Dictionary<string, Upgrade> upgrades)
	{
		upgrades.Add(ID, this);
	}

	public virtual void RemoveFrom(IUpgradeable upgradeable)
	{
		RemoveFrom(upgradeable.Upgrades);
	}

	public virtual void RemoveFrom(Dictionary<string, Upgrade> upgrades)
	{
		upgrades.Remove(ID);
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