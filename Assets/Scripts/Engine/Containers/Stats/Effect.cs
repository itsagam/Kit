using System;

namespace Engine.Containers
{
	public enum EffectType
	{
		Value,
		Percentage,
		Multiplier
	}

	[Serializable]
	public struct Effect
	{
		public string Stat;
		public EffectType Type;
		public float Value;

		public Effect(string stat, string value)
		{
			Stat = stat;
			(Type, Value) = Parse(value);
		}

		public Effect(string stat, EffectType type, float value)
		{
			Stat = stat;
			Type = type;
			Value = value;
		}

		public override string ToString()
		{
			return Stat + ": " + Convert(this);
		}

		public static (EffectType, float) Parse(string str)
		{
			(EffectType type, float value) output;
			if (str.StartsWith("x", StringComparison.Ordinal))
			{
				output.type = EffectType.Multiplier;
				output.value = System.Convert.ToSingle(str.Substring(1));
			}
			else if (str.EndsWith("%", StringComparison.Ordinal))
			{
				output.type = EffectType.Percentage;
				output.value = System.Convert.ToSingle(str.Substring(0, str.Length - 1));
			}
			else
			{
				output.type = EffectType.Value;
				output.value = System.Convert.ToSingle(str);
			}
			return output;
		}

		public static string Convert(Effect effect)
		{
			string output = "";

			if (effect.Type != EffectType.Multiplier && effect.Value > 0)
				output += "+";

			switch (effect.Type)
			{
				case EffectType.Value:
					output += effect.Value;
					break;

				case EffectType.Multiplier:
					output += "x" + effect.Value;
					break;

				case EffectType.Percentage:
					output += effect.Value + "%";
					break;
			}
			return output;
		}
	}
}