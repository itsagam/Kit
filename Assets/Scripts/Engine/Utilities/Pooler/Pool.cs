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
	Interface,
	SendMessage,
	BroadcastMessage
}

public class Pool : MonoBehaviour, IEnumerable<Component>
{
	public const string SpawnedMessage = "OnSpawned";
	public const string DespawnedMessage = "OnDespawned";

	public Component Prefab;
	public PoolMessageMode MessageMode = PoolMessageMode.Interface;

	[ToggleGroup("Preload")]
	public bool Preload = false;

	[ToggleGroup("Preload")]
	[LabelText("Amount")]
	[MinValue(1)]
	public int PreloadAmount = 5;

	[ToggleGroup("Preload")]
	[HideIf("PreloadAmount")]
	[LabelText("Delay")]
	[SuffixLabel("seconds", true)]
	[MinValue(0)]
	public float PreloadDelay = 0.0f;

	[ToggleGroup("Preload")]
	[LabelText("Time")]
	[SuffixLabel("seconds", true)]
	[MinValue(0)]
	public float PreloadTime = 1.0f;

	public bool Organize = true;
	public bool Persistent = false;

	protected List<Component> instances = new List<Component>();

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
		int index = instances.FindIndex(c => c != null);
		if (index >= 0)
		{
			instance = instances[index];
			instance.gameObject.SetActive(true);
			instances.RemoveAt(index);
		}
		else
		{
			instance = Instantiate(Prefab);
			instance.name = name;
			if (Organize)
				instance.transform.SetParent(transform);	
		}

		switch (MessageMode)
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
	
		return instance;
	}

	public Component Spawn(Transform parent, bool worldPositionStays = false)
	{
		var instance = Spawn();
		instance.transform.SetParent(parent, worldPositionStays);
		return instance;
	}

	public Component Spawn(Vector3 position)
	{
		var instance = Spawn();
		instance.transform.position = position;
		return instance;
	}

	public Component Spawn(Vector3 position, Quaternion rotation)
	{
		var instance = Spawn();
		var trans = instance.transform;
		trans.position = position;
		trans.rotation = rotation;
		return instance;
	}

	public Component Spawn(Vector3 position, Quaternion rotation, Transform parent)
	{
		var instance = Spawn();
		var trans = instance.transform;
		trans.parent = parent;
		trans.position = position;
		trans.rotation = rotation;
		return instance;
	}

	public T Spawn<T>() where T: Component
	{
		return (T) Spawn();
	}

	public void Despawn(Component instance)
	{
		switch (MessageMode)
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
		instance.gameObject.SetActive(false);
		instances.Add(instance);
	}

	public IEnumerator<Component> GetEnumerator()
	{
		return instances.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return instances.GetEnumerator();
	}
}