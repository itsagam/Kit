using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector.Editor;

public class BuffDrawer : OdinValueDrawer<Buff>
{
	protected override void DrawPropertyLayout(GUIContent label)
	{
		SirenixEditorGUI.BeginToolbarBox(label);

		Property.Children["ID"].Draw();

		SirenixEditorGUI.BeginHorizontalPropertyLayout(Property.Children["Time"].Label);
		Property.Children["Time"].Draw(null);
		Property.Children["Mode"].Draw(null);
		SirenixEditorGUI.EndHorizontalPropertyLayout();

		Property.Children["Effects"].Draw();

		SirenixEditorGUI.EndToolbarBox();
	}
}
#endif

public enum BuffMode
{
	Add,
	Extend,
	Keep,
	Replace,
	Longer,
	Shorter,
}

[Serializable]
public class Buff : Upgrade
{
	public float Time;
	public BuffMode Mode = BuffMode.Extend;

	public Buff()
		: base()
	{
	}

	public Buff(string id)
		: base(id)
	{
	}

	public Buff(IEnumerable<Effect> effects, float time, BuffMode mode = BuffMode.Extend)
		: base(effects)
	{
		Time = time;
		Mode = mode;
	}

	public Buff(string id, IEnumerable<Effect> effects, float time, BuffMode mode = BuffMode.Extend)
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
				try
				{
					base.RemoveFrom(upgradeable);
				}
				catch { }
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