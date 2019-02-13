using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector.Editor;

public class BuffDrawer : OdinValueDrawer<Buff>
{
	protected override void DrawPropertyLayout(GUIContent label)
	{
		SirenixEditorGUI.BeginToolbarBox(label);

		Property.Children["ID"].Draw();
		if (! Application.isPlaying)
		{
			SirenixEditorGUI.BeginHorizontalPropertyLayout(Property.Children["Time"].Label);
			Property.Children["Time"].Draw(null);
			Property.Children["Mode"].Draw(null);
			SirenixEditorGUI.EndHorizontalPropertyLayout();
		}
		Property.Children["Effects"].Draw();

		SirenixEditorGUI.EndToolbarBox();
	}
}
#endif

public enum BuffMode
{
	Nothing,
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
	{
	}

	public Buff(IEnumerable<Effect> effects, float time, BuffMode mode = BuffMode.Extend)
	{
		Effects.AddRange(effects);
		ID = ToString();
		Time = time;
		Mode = mode;
	}

	public Buff(string id, IEnumerable<Effect> effects, float time, BuffMode mode = BuffMode.Extend)
	{
		Effects.AddRange(effects);
		ID = id;
		Time = time;
		Mode = mode;
	}

	public virtual Buff AddTo(IUpgradeable upgradeable)
	{
		return AddTo(upgradeable, Mode);
	}

	public virtual Buff AddTo(IUpgradeable upgradeable, BuffMode mode)
	{
		Buff previous = null;
		if (mode != BuffMode.Nothing)
			previous = Find(upgradeable, ID) as Buff;
			
		if (mode == BuffMode.Nothing || previous == null)
		{
			upgradeable.GetUpgrades().Add(this);
			Observable.Timer(TimeSpan.FromSeconds(Time)).Subscribe(l =>
			{
				try
				{
					upgradeable.GetUpgrades().Remove(this);
				}
				catch { }
			});
			return this;
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
		return this;
	}

	public virtual bool RemoveFrom(IUpgradeable upgradeable)
	{
		return upgradeable.GetUpgrades().Remove(this);
	}
}