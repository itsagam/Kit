using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector.Editor;

public class EffectDrawer : OdinValueDrawer<Effect>
{
	protected override void DrawPropertyLayout(GUIContent label)
	{
		SirenixEditorGUI.BeginHorizontalPropertyLayout(label);
		Effect effect = ValueEntry.SmartValue;

		effect.Stat = SirenixEditorGUI.DynamicPrimitiveField(null, effect.Stat, GUILayout.MaxWidth(140));

		string input = SirenixEditorGUI.DynamicPrimitiveField(null, Effect.Convert(effect));
		try {
			(effect.Type, effect.Value) = Effect.Parse(input);
		}
		catch {}

		ValueEntry.SmartValue = effect;
		SirenixEditorGUI.EndHorizontalPropertyLayout();
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