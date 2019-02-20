﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Async;
using Sirenix.OdinInspector;

public interface IPooled
{
	void AwakeFromPool();
	void OnDestroyIntoPool();
}

public enum PoolMessageMode
{
	None,
	Interface,
	SendMessage,
	BroadcastMessage
}

public enum PoolLimitMode
{
	StopGiving,
	ReuseFirst,
	DestroyAfterUse,
}

[AddComponentMenu("Pooling/Pool")]
public class Pool : MonoBehaviour, IEnumerable<Component>
{
	public const string InstantiateMessage = "AwakeFromPool";
	public const string DestroyMessage = "OnDestroyIntoPool";
	private const int UnlimitedMaxPreloadAmount = 250;

	protected LinkedList<Component> availableInstances = new LinkedList<Component>();
	protected LinkedList<Component> usedInstances = new LinkedList<Component>();

	#region Properties
	[ReadOnly]
	[HideInInlineEditors]
	[ShowIf("ShowGroup")]
	public PoolGroup Group;

	[Required]
	[ValueDropdown("GetComponents", AppendNextDrawer = true)]
	[OnValueChanged("ResetName")]
	public Component Prefab;

	[HideInInlineEditors]
	[LabelText("Message")]
	public PoolMessageMode MessageMode = PoolMessageMode.None;

	[ToggleGroup("Preload")]
	public bool Preload = false;

	[ToggleGroup("Preload")]
	[LabelText("Amount")]
	[PropertyRange(0, "MaxPreloadAmount")]
	public int PreloadAmount = 5;

	[ToggleGroup("Preload")]
	[LabelText("Delay")]
	[SuffixLabel("seconds", true)]
	[MinValue(0)]
	public float PreloadDelay = 0.0f;

	[ToggleGroup("Preload")]
	[LabelText("Time")]
	[SuffixLabel("seconds", true)]
	[MinValue(0)]
	public float PreloadTime = 1.0f;

	[ToggleGroup("Limit")]
	[OnValueChanged("ClampPreloadAmount")]
	public bool Limit = true;

	[ToggleGroup("Limit")]
	[LabelText("Mode")]
	public PoolLimitMode LimitMode = PoolLimitMode.DestroyAfterUse;
	
	[ToggleGroup("Limit")]
	[LabelText("Amount")]
	[MinValue(0)]
	[OnValueChanged("ClampPreloadAmount")]
	public int LimitAmount = 50;

	[HideInInlineEditors]
	public bool Organize = true;
	
	[HideInInlineEditors]
	[ShowIf("ShowPersistent")]
	public bool Persistent = false;
	#endregion

	public bool IsDestroying { get; protected set; }
	protected Transform transformCached;

	#region Initialization
	protected void Awake()
	{
		Pooler.CachePool(this);
		transformCached = transform;

		if (Preload)
			PreloadInstances().Forget();
	}

	protected void Start()
	{
		if (Persistent && transformCached.parent == null)
			DontDestroyOnLoad(gameObject);
	}

	protected void OnDestroy()
	{
		IsDestroying = true;
		if (Group != null && !Group.IsDestroying)
			Group.Pools.Remove(this);
		Pooler.UncachePool(this);
	}
	
	public async UniTask PreloadInstances()
	{
		if (PreloadDelay > 0)
			await Observable.Timer(TimeSpan.FromSeconds(PreloadDelay));

		if (PreloadTime <= 0)
		{
			PreloadInstances(PreloadAmount);
			return;
		}

		int amount = PreloadAmount;
		float preloadFrames = PreloadTime / Time.fixedUnscaledDeltaTime;
		float amountPerFrame = PreloadAmount / preloadFrames;

		while (amount > 0)
		{
			float runningAmount = 0;
			await Observable.EveryUpdate().TakeWhile(l => runningAmount < 1).Do(l => runningAmount += amountPerFrame);

			int runningCount = (int) runningAmount;
			PreloadInstances(runningCount);
			amount -= runningCount;
		}
	}

	protected void PreloadInstances(int count)
	{
		for (int i = 0; i < count; i++)
		{
			var instance = CreateInstance();
			instance.gameObject.SetActive(false);
			availableInstances.AddLast(instance);
		}
	}
	#endregion

	#region Instantiate/Destroy
	protected Component CreateInstance()
	{
		Component instance = GameObject.Instantiate(Prefab);
		instance.name = name;

		var poolInstance = instance.gameObject.AddComponent<PoolInstance>();
		poolInstance.Pool = this;
		poolInstance.Component = instance;

		if (Organize)
			instance.transform.SetParent(transformCached);

		return instance;
	}

	public Component Instantiate()
	{
		Component instance = null;
		if (availableInstances.Count > 0)
		{
			instance = availableInstances.First.Value;
			instance.gameObject.SetActive(true);
			availableInstances.RemoveFirst();
		}
		else
		{
			if (Limit && Overlimit)
			{
				switch (LimitMode)
				{
					case PoolLimitMode.StopGiving:
						return null;

					case PoolLimitMode.ReuseFirst:
						instance = usedInstances.First.Value;
						usedInstances.RemoveFirst();
						usedInstances.AddLast(instance);
						SendDestroyMessage(instance);
						SendInstantiateMessage(instance);
						return instance;
				}				
			}

			instance = CreateInstance();
		}
		usedInstances.AddLast(instance);
		SendInstantiateMessage(instance);
		return instance;
	}

	public Component Instantiate(Transform parent, bool worldPositionStays = false)
	{
		var instance = Instantiate();
		if (instance != null)
			instance.transform.SetParent(parent, worldPositionStays);
		return instance;
	}

	public Component Instantiate(Vector3 position)
	{
		var instance = Instantiate();
		if (instance != null)
			instance.transform.position = position;
		return instance;
	}

	public Component Instantiate(Vector3 position, Quaternion rotation)
	{
		var instance = Instantiate();
		if (instance != null)
		{
			var trans = instance.transform;
			trans.position = position;
			trans.rotation = rotation;
		}
		return instance;
	}

	public Component Instantiate(Vector3 position, Quaternion rotation, Transform parent)
	{
		var instance = Instantiate();
		if (instance != null)
		{
			var trans = instance.transform;
			trans.parent = parent;
			trans.position = position;
			trans.rotation = rotation;
		}
		return instance;
	}

	public T Instantiate<T>() where T: Component
	{
		return (T) Instantiate();
	}

	public T Instantiate<T>(Transform parent, bool worldPositionStays = false) where T : Component
	{
		return (T) Instantiate(parent, worldPositionStays);
	}

	public T Instantiate<T>(Vector3 position) where T : Component
	{
		return (T) Instantiate(position);
	}

	public T Instantiate<T>(Vector3 position, Quaternion rotation) where T : Component
	{
		return (T) Instantiate(position, rotation);
	}

	public T Instantiate<T>(Vector3 position, Quaternion rotation, Transform parent) where T : Component
	{
		return (T) Instantiate(position, rotation, parent);
	}

	public void Destroy(Component instance)
	{
		usedInstances.Remove(instance);
		if (Limit && Overlimit && LimitMode == PoolLimitMode.DestroyAfterUse)
		{
			GameObject.Destroy(instance.gameObject);
			return;
		}
		availableInstances.AddLast(instance);
		instance.gameObject.SetActive(false);
		SendDestroyMessage(instance);
	}

	public void DestroyAll()
	{
		foreach (var instance in usedInstances.Reverse())
			Destroy(instance);
	}
	#endregion

	#region Helper methods
	protected void SendInstantiateMessage(Component instance)
	{
		switch (MessageMode)
		{
			case PoolMessageMode.Interface:
				if (instance is IPooled pooled)
					pooled.AwakeFromPool();
				break;

			case PoolMessageMode.SendMessage:
				instance.gameObject.SendMessage(InstantiateMessage, SendMessageOptions.DontRequireReceiver);
				break;

			case PoolMessageMode.BroadcastMessage:
				instance.gameObject.BroadcastMessage(InstantiateMessage, SendMessageOptions.DontRequireReceiver);
				break;
		}
	}

	protected void SendDestroyMessage(Component instance)
	{
		switch (MessageMode)
		{
			case PoolMessageMode.Interface:
				if (instance is IPooled pooled)
					pooled.OnDestroyIntoPool();
				break;

			case PoolMessageMode.SendMessage:
				instance.gameObject.SendMessage(DestroyMessage, SendMessageOptions.DontRequireReceiver);
				break;

			case PoolMessageMode.BroadcastMessage:
				instance.gameObject.BroadcastMessage(DestroyMessage, SendMessageOptions.DontRequireReceiver);
				break;
		}
	}

	public IEnumerator<Component> GetEnumerator()
	{
		return usedInstances.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return usedInstances.GetEnumerator();
	}

	protected bool Overlimit
	{
		get
		{
			return UsedCount >= LimitAmount;
		}
	}
	#endregion

	#region Editor functionality
	#if UNITY_EDITOR
	private IEnumerable<Component> GetComponents()
	{
		if (Prefab == null)
			return Enumerable.Empty<Component>();

		return Prefab.gameObject.GetComponents<Component>();
	}

	private void ResetName()
	{
		if (Prefab != null)
			name = Prefab.name;
	}

	private void ClampPreloadAmount()
	{
		if (PreloadAmount > MaxPreloadAmount)
			PreloadAmount = MaxPreloadAmount;
	}

	private bool ShowGroup
	{
		get
		{
			return Group != null;
		}
	}

	private bool ShowPersistent
	{
		get
		{
			return transform.parent == null;
		}
	}

	private int MaxPreloadAmount
	{
		get
		{
			return Limit ? LimitAmount : UnlimitedMaxPreloadAmount;
		}
	}
	#endif
	#endregion

	#region Public fields
	[PropertySpace]

	[EnableGUI]
	[ShowInInspector]
	[HideInInlineEditors]
	public LinkedList<Component> Available
	{
		get
		{
			return availableInstances;
		}
	}

	public int AvailableCount
	{
		get
		{
			return availableInstances.Count;
		}
	}

	[EnableGUI]
	[ShowInInspector]
	[HideInInlineEditors]
	public LinkedList<Component> Used
	{
		get
		{
			return usedInstances;
		}
	}

	public int UsedCount
	{
		get
		{
			return usedInstances.Count;
		}
	}
	#endregion
}