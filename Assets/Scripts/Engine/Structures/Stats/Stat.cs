using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;

public class StatDrawer : OdinValueDrawer<Stat>
{
	protected override void Initialize()
	{
		base.Initialize();
		Property.Children["parent"].ValueEntry.WeakSmartValue = Property.Tree.UnitySerializedObject.targetObject;
		Property.Children["property"].ValueEntry.WeakSmartValue = Property.Name;
	}

	protected override void DrawPropertyLayout(GUIContent label)
	{
		Property.Children["Base"].Draw(label);
	}
}
#endif

[Serializable]
public class Stat : ISerializationCallbackReceiver, IDisposable
{
	public StatReactiveProperty Base = new StatReactiveProperty();

	[SerializeField]
	protected UnityEngine.Object parent;

	[SerializeField]
	protected string property;

	public ReadOnlyReactiveProperty<float> Current { get; protected set; }

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		if (Current == null
			&& !property.IsNullOrEmpty()
			&& parent is IUpgradeable upgradeable)
		{
			Current = Stats.CreateCurrentProperty(Base, upgradeable.Upgrades, property);
		}
	}

	public float BaseValue
	{
		get
		{
			return Base.Value;
		}
		set
		{
			Base.Value = value;
		}
	}

	public float CurrentValue
	{
		get
		{
			return Current.Value;
		}
	}

	public void Dispose()
	{
		Base.Dispose();
		Current?.Dispose();
	}
}