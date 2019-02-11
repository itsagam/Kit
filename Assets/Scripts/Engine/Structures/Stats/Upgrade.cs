using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

[Serializable]
public class Upgrade
{	
	public string ID;
	public List<Effect> Effects;

	public Upgrade()
	{
	}

	public Upgrade(string id)
	{
		ID = id;
	}

	public Upgrade(IEnumerable<Effect> effects)
	{
		Effects.AddRange(effects);
		ID = ToString();
	}

	public Upgrade(string id, IEnumerable<Effect> effects)
	{
		Effects.AddRange(effects);
		ID = id;
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

	public virtual void AddTo(IUpgradeable upgradeable)
	{
		upgradeable.Upgrades.Add(ID, this);
	}

	public virtual void RemoveFrom(IUpgradeable upgradeable)
	{
		upgradeable.Upgrades.Remove(ID);
	}

	public override string ToString()
	{
		string output = "";
		foreach (var effect in Effects)
			output += effect.ToString() + "\n";
		return output;
	}
}