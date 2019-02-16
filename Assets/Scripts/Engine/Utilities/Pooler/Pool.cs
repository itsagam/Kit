using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Async;
using Sirenix.OdinInspector;

public interface IPooled
{
	void OnSpawned();
	void OnDespawned();
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
	StopSpawning,
	DespawnFirst,
	DestroyAfterUse,
}

public class Pool : MonoBehaviour, IEnumerable<Component>
{
	public const int UnlimitedMaxPreloadAmount = 250;

	public const string SpawnedMessage = "OnSpawned";
	public const string DespawnedMessage = "OnDespawned";

	public Component Prefab;
	public PoolMessageMode Message = PoolMessageMode.None;

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

	public bool Organize = true;
	public bool Persistent = false;

	protected LinkedList<Component> availableInstances = new LinkedList<Component>();
	protected LinkedList<Component> usedInstances = new LinkedList<Component>();

	protected void Awake()
	{
		Pooler.CachePool(this);
		if (Persistent)
			DontDestroyOnLoad(gameObject);
	}

	protected void Start()
	{
		if (Preload)
			PreloadInstances().Forget();
	}

	protected void OnDestroy()
	{
		Pooler.UncachePool(this);
	}

	protected async UniTask PreloadInstances()
	{
		if (PreloadDelay > 0)
			await Observable.Timer(TimeSpan.FromSeconds(PreloadDelay));

		int amount = PreloadAmount;
		float preloadFrames = PreloadTime / Time.fixedUnscaledDeltaTime;
		float amountPerFrame = PreloadAmount / preloadFrames;

		while (amount > 0)
		{
			float runningAmount = 0;
			await Observable.EveryUpdate().TakeWhile(l => runningAmount < 1).Do(l => runningAmount += amountPerFrame);

			int runningCount = (int) runningAmount;
			for (int i = 0; i < runningCount; i++)
				Spawn();

			amount -= runningCount;
		}
	}

	public Component Spawn()
	{
		Component instance;
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
					case PoolLimitMode.StopSpawning:
						return null;

					case PoolLimitMode.DespawnFirst:
						instance = usedInstances.First.Value;
						usedInstances.RemoveFirst();
						usedInstances.AddLast(instance);
						SendDespawnedMessage(instance);
						SendSpawnedMessage(instance);
						return instance;
				}				
			}

			instance = GameObject.Instantiate(Prefab);
			instance.name = name;
			if (Organize)
				instance.transform.SetParent(transform);
		}
		usedInstances.AddLast(instance);
		SendSpawnedMessage(instance);
		return instance;
	}

	public Component Spawn(Transform parent, bool worldPositionStays = false)
	{
		var instance = Spawn();
		if (instance != null)
			instance.transform.SetParent(parent, worldPositionStays);
		return instance;
	}

	public Component Spawn(Vector3 position)
	{
		var instance = Spawn();
		if (instance != null)
			instance.transform.position = position;
		return instance;
	}

	public Component Spawn(Vector3 position, Quaternion rotation)
	{
		var instance = Spawn();
		if (instance != null)
		{
			var trans = instance.transform;
			trans.position = position;
			trans.rotation = rotation;
		}
		return instance;
	}

	public Component Spawn(Vector3 position, Quaternion rotation, Transform parent)
	{
		var instance = Spawn();
		if (instance != null)
		{
			var trans = instance.transform;
			trans.parent = parent;
			trans.position = position;
			trans.rotation = rotation;
		}
		return instance;
	}

	public T Spawn<T>() where T: Component
	{
		return (T) Spawn();
	}

	public void Despawn(Component instance)
	{
		usedInstances.Remove(instance);
		if (Limit && Overlimit && LimitMode == PoolLimitMode.DestroyAfterUse)
		{
			GameObject.Destroy(instance.gameObject);
			return;
		}
		availableInstances.AddLast(instance);
		instance.gameObject.SetActive(false);
		SendDespawnedMessage(instance);
	}

	protected void SendSpawnedMessage(Component instance)
	{
		switch (Message)
		{
			case PoolMessageMode.Interface:
				if (instance is IPooled pooled)
					pooled.OnSpawned();
				break;

			case PoolMessageMode.SendMessage:
				instance.gameObject.SendMessage(SpawnedMessage, SendMessageOptions.DontRequireReceiver);
				break;

			case PoolMessageMode.BroadcastMessage:
				instance.gameObject.BroadcastMessage(SpawnedMessage, SendMessageOptions.DontRequireReceiver);
				break;
		}
	}

	protected void SendDespawnedMessage(Component instance)
	{
		switch (Message)
		{
			case PoolMessageMode.Interface:
				if (instance is IPooled pooled)
					pooled.OnDespawned();
				break;

			case PoolMessageMode.SendMessage:
				instance.gameObject.SendMessage(DespawnedMessage, SendMessageOptions.DontRequireReceiver);
				break;

			case PoolMessageMode.BroadcastMessage:
				instance.gameObject.BroadcastMessage(DespawnedMessage, SendMessageOptions.DontRequireReceiver);
				break;
		}
	}

	public bool Overlimit
	{
		get
		{
			return usedInstances.Count >= LimitAmount;
		}
	}

	[PropertySpace]

	[ShowInInspector]
	[EnableGUI]
	public LinkedList<Component> Available
	{
		get
		{
			return availableInstances;
		}
	}

	[ShowInInspector]
	[EnableGUI]
	public LinkedList<Component> Used
	{
		get
		{
			return usedInstances;
		}
	}

	public void ClampPreloadAmount()
	{
		if (PreloadAmount > MaxPreloadAmount)
			PreloadAmount = MaxPreloadAmount;
	}

	public int MaxPreloadAmount
	{
		get
		{
			return Limit ? LimitAmount : UnlimitedMaxPreloadAmount;
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
}