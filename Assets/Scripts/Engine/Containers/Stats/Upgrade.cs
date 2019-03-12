﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UniRx;
using UnityEngine;

namespace Engine.Containers
{
#if UNITY_EDITOR
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

		public Upgrade(string id)
		{
			ID = id;
		}

		public Upgrade(string id, IEnumerable<Effect> effects)
			: this(id)
		{
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
			foreach (Effect effect in Effects)
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
			if (previous == null)
				return false;

			upgradeable.GetUpgrades().Remove(previous);
			return true;
		}
	}
}