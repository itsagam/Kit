using System;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Engine.Containers
{
#if UNITY_EDITOR
	public class EffectDrawer : OdinValueDrawer<Effect>
	{
		public const float EffectWidth = 120;

		protected override void DrawPropertyLayout(GUIContent label)
		{
			SirenixEditorGUI.BeginIndentedHorizontal();
			Effect effect = ValueEntry.SmartValue;
			effect.Stat = SirenixEditorGUI.DynamicPrimitiveField(label, effect.Stat);
			string input = SirenixEditorGUI.DynamicPrimitiveField(null, Effect.Convert(effect), GUILayout.MaxWidth(EffectWidth));
			try {
				(effect.Type, effect.Value) = Effect.Parse(input);
			}
			catch {}

			ValueEntry.SmartValue = effect;
			SirenixEditorGUI.EndIndentedHorizontal();
		}
	}
#endif

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
			if (str.StartsWith("x"))
			{
				output.type = EffectType.Multiplier;
				output.value = System.Convert.ToSingle(str.Substring(1));
			}
			else if (str.EndsWith("%"))
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