using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector.Editor;

public class UpgradeDrawer : OdinValueDrawer<Upgrade>
{
	protected override void DrawPropertyLayout(GUIContent label)
	{
		SirenixEditorGUI.BeginToolbarBox(label);	
		CallNextDrawer(null);
		SirenixEditorGUI.EndToolbarBox();
	}
}
#endif

public interface IUpgradeable
{
	ReactiveCollection<Upgrade> GetUpgrades();
}

[Serializable]
public class Upgrade
{
	public string ID;
	public List<Effect> Effects = new List<Effect>();

	public Upgrade()
	{
	}

	public Upgrade(string id, IEnumerable<Effect> effects)
	{
		ID = id;
		AddEffects(effects);
	}

	public void AddEffects(IEnumerable<Effect> effects)
	{
		Effects.AddRange(effects);
	}

	public void AddEffect(Effect effect)
	{
		Effects.Add(effect);
	}

	public void AddEffect(string stat, string value)
	{
		Effects.Add(new Effect(stat, value));
	}

	public void AddEffect(string stat, EffectType type, float value)
	{
		Effects.Add(new Effect(stat, type, value));
	}

	public void RemoveEffect(Effect effect)
	{
		Effects.Remove(effect);
	}

	public void RemoveEffects(string stat)
	{
		Effects.RemoveAll(e => e.Stat == stat);
	}

	public override string ToString()
	{
		string output = "";
		foreach (var effect in Effects)
			output += effect.ToString() + "\n";
		return output;
	}

	public static Upgrade Find(IUpgradeable upgradeable, string id)
	{
		return upgradeable.GetUpgrades().FirstOrDefault(b => b.ID == id);
	}

	public static bool RemoveFrom(IUpgradeable upgradeable, string id)
	{
		Upgrade previous = Find(upgradeable, id);
		if (previous != null)
		{
			upgradeable.GetUpgrades().Remove(previous);
			return true;
		}
		return false;
	}
}