using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Kit.Pooling
{
	/// <summary>How an instance gets informed of pooling events?</summary>
	public enum PoolMessageMode
	{
		/// <summary>Do not inform of pooling events.</summary>
		None,

		/// <summary>Call <see cref="IPooled" /> methods on just the instance component.</summary>
		IPooledComponent,

		/// <summary>Call <see cref="IPooled" /> methods on the all instance <see cref="GameObject" /> components.</summary>
		IPooledGameObject,

		/// <summary>Call <see cref="IPooled" /> methods directly on the instance <see cref="GameObject" /> hierarchy.</summary>
		IPooledHierarchy,

		/// <summary>Call <see cref="Component.SendMessage(string)" /> on instance <see cref="GameObject" />s.</summary>
		SendMessage,

		/// <summary>Call <see cref="Component.BroadcastMessage(string)" /> on instance  <see cref="GameObject" />s.</summary>
		BroadcastMessage
	}

	/// <summary>What to do when a pool reaches its limit?</summary>
	public enum PoolLimitMode
	{
		/// <summary>Stop giving new instances.</summary>
		StopGiving,

		/// <summary>Reuse the first instance given.</summary>
		ReuseFirst,

		/// <summary>Create a new instance but do not pool it when it gets destroyed.</summary>
		DestroyAfterUse
	}

	/// <summary>A Pool represents all the instances of a particular component (and its <see cref="GameObject" />).</summary>
	[AddComponentMenu("Pooling/Pool")]
	public class Pool: MonoBehaviour, IEnumerable<Component>
	{
		#region Fields

		/// <summary>
		///     Message to send on instance initialization when <see cref="PoolMessageMode" /> is <see cref="PoolMessageMode.SendMessage" /> or
		///     <see cref="PoolMessageMode.BroadcastMessage" />.
		/// </summary>
		public const string InstantiateMessage = "AwakeFromPool";

		/// <summary>
		///     Message to send on instance pooling when <see cref="PoolMessageMode" /> is <see cref="PoolMessageMode.SendMessage" /> or
		///     <see cref="PoolMessageMode.BroadcastMessage" />.
		/// </summary>
		public const string DestroyMessage = "OnDestroyIntoPool";

		private const int MaxPreloadLimit = 250;

		/// <summary>
		///     The prefab to pool instances of. The particular component is important as that's the component you will get when initializing an
		///     instance.
		/// </summary>
		[Required]
		[ValueDropdown("GetPrefabComponents", DropdownTitle = "Component", AppendNextDrawer = true)]
		[OnValueChanged("ResetName")]
		[Tooltip("The prefab to pool instances of. The particular component is important as that's the component you will get when " +
				 "initializing an instance.")]
		public Component Prefab;

		// /// <summary>The pool group this pool belongs to.</summary>
		// [HideInInlineEditors]
		// [SceneObjectsOnly]
		// [OnValueChanged("OnGroupChanged")]
		// [Tooltip("The pool group this pool belongs to.")]
		// public PoolGroup Group;

		/// <summary>How an instance gets informed of pooling events?</summary>
		[HideInInlineEditors]
		[LabelText("Message")]
		[Tooltip("How an instance gets informed of pooling events?")]
		public PoolMessageMode MessageMode = PoolMessageMode.IPooledComponent;

		/// <summary>Whether to pre-instantiate a certain of number of instances for future use.</summary>
		[ToggleGroup("Preload")]
		[Tooltip("Whether to pre-instantiate a certain of number of instances for future use.")]
		public bool Preload = false;

		/// <summary>How many instances to instantiate for pre-loading?</summary>
		[ToggleGroup("Preload")]
		[LabelText("Amount")]
		[Tooltip("How many instances to instantiate for pre-loading?")]
		[PropertyRange(0, "MaxPreloadAmount")]
		public int PreloadAmount = 10;

		/// <summary>How many seconds to wait before starting to pre-load?</summary>
		[ToggleGroup("Preload")]
		[LabelText("Delay")]
		[Tooltip("How many seconds to wait before starting to pre-load?")]
		[SuffixLabel("seconds", true)]
		[MinValue(0)]
		public float PreloadDelay = 1.0f;

		/// <summary>How many seconds to divide the pre-loading over?</summary>
		[ToggleGroup("Preload")]
		[LabelText("Time")]
		[Tooltip("How many seconds to divide the pre-loading over?")]
		[SuffixLabel("seconds", true)]
		[MinValue(0)]
		public float PreloadTime = 3.0f;

		/// <summary>Limit the pool to a certain number of instances.</summary>
		[ToggleGroup("Limit")]
		[Tooltip("Limit the pool to a certain number of instances.")]
		[OnValueChanged("ClampPreloadAmount")]
		public bool Limit = true;

		/// <summary>What to do when the pool limit is reached?</summary>
		[ToggleGroup("Limit")]
		[LabelText("Mode")]
		[Tooltip("What to do when the pool limit is reached?")]
		public PoolLimitMode LimitMode = PoolLimitMode.DestroyAfterUse;

		/// <summary>Number of instances to cap the pool to.</summary>
		[ToggleGroup("Limit")]
		[LabelText("Amount")]
		[MinValue(0)]
		[Tooltip("Number of instances to cap the pool to.")]
		[OnValueChanged("ClampPreloadAmount")]
		public int LimitAmount = 50;

		/// <summary>
		///     Whether to organize instances for a cleaner scene hierarchy? Disable this if you want to set the parent of your instances
		///     manually.
		/// </summary>
		[Tooltip("Whether to organize instances for a cleaner scene hierarchy? Disable this if you want to set the parent of your " +
				 "instances manually.")]
		[HideInInlineEditors]
		public bool Organize = true;

		/// <summary>Keep this pool persistent across scenes.</summary>
		[HideInInlineEditors]
		[ShowIf("ShowPersistent")]
		[Tooltip("Keep this pool persistent across scenes.")]
		public bool Persistent = false;

		/// <summary>Returns whether the pool is being destroyed.</summary>
		public bool IsDestroying { get; protected set; }

		/// <summary>A list of all instances that can be re-used.</summary>
		[PropertySpace]
		[ShowInInspector]
		[HideInInlineEditors]
		[ReadOnly]
		public readonly LinkedList<Component> Available = new LinkedList<Component>();

		/// <summary>A list of all instances that are in use.</summary>
		[ShowInInspector]
		[HideInInlineEditors]
		[ReadOnly]
		public readonly LinkedList<Component> Used = new LinkedList<Component>();

		protected new Transform transform;

		#endregion

		#region Initialization

		protected void Awake()
		{
			transform = base.transform;

			if (Prefab != null)
				Pooler.PoolsByPrefab.Add(Prefab, this);
			Pooler.PoolsByName.Add(name, this);

			if (Preload)
				PreloadInstances().Forget();
		}

		protected void Start()
		{
			if (Persistent && transform.parent == null)
				DontDestroyOnLoad(gameObject);
		}

		protected void OnDestroy()
		{
			IsDestroying = true;
			// if (Group != null && !Group.IsDestroying)
			// 	Group.Pools.Remove(this);
			if (Prefab != null)
				Pooler.PoolsByPrefab.Remove(Prefab);
			Pooler.PoolsByName.Remove(name);
		}

		/// <summary>Start preloading instances. Automatically gets called if <see cref="Preload" /> is true.</summary>
		public async UniTask PreloadInstances()
		{
			if (PreloadDelay > 0)
				await UniTask.Delay(TimeSpan.FromSeconds(PreloadDelay));

			if (PreloadTime <= 0)
			{
				PreloadInstances(PreloadAmount);
				return;
			}

			int amount = PreloadAmount;
			float preloadFrames = PreloadTime    / Time.fixedUnscaledDeltaTime;
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
				Component instance = CreateInstance();
				instance.gameObject.SetActive(false);
				Available.AddLast(instance);
			}
		}

		#endregion

		#region Instantiate/Destroy

		protected Component CreateInstance()
		{
			Component instance = Instantiate(Prefab);
			instance.name = Prefab.name;

			GameObject go = instance.gameObject;
			go.AddComponent<PoolInstance>();
			Pooler.InfoByGameObject.Add(go, new PoolInstanceInfo(this, instance));

			if (Organize)
				instance.transform.SetParent(transform);

			return instance;
		}

		/// <summary>Initialize a pool instance.</summary>
		/// <returns>The instance given.</returns>
		public Component Instantiate()
		{
			Component instance;
			if (Available.Count > 0)
			{
				instance = Available.First.Value;
				instance.gameObject.SetActive(true);
				Available.RemoveFirst();
			}
			else
			{
				if (Limit && Overlimit)
					switch (LimitMode)
					{
						case PoolLimitMode.StopGiving:
							return null;

						case PoolLimitMode.ReuseFirst:
							instance = Used.First.Value;
							Pooler.GetInstanceInfo(instance).IsPooled = false;
							Used.RemoveFirst();
							Used.AddLast(instance);
							SendDestroyMessage(instance);
							SendInstantiateMessage(instance);
							return instance;
					}

				instance = CreateInstance();
			}

			Pooler.GetInstanceInfo(instance).IsPooled = false;
			Used.AddLast(instance);
			SendInstantiateMessage(instance);
			return instance;
		}

		/// <inheritdoc cref="Instantiate()"/>
		public Component Instantiate(Transform parent, bool worldSpace = false)
		{
			Component instance = Instantiate();
			if (instance != null)
				instance.transform.SetParent(parent, worldSpace);
			return instance;
		}

		/// <inheritdoc cref="Instantiate()"/>
		public Component Instantiate(Vector3 position)
		{
			Component instance = Instantiate();
			if (instance != null)
				instance.transform.position = position;
			return instance;
		}

		/// <inheritdoc cref="Instantiate()"/>
		public Component Instantiate(Vector3 position, Quaternion rotation)
		{
			Component instance = Instantiate();
			if (instance == null)
				return instance;
			Transform trans = instance.transform;
			trans.position = position;
			trans.rotation = rotation;
			return instance;
		}

		/// <inheritdoc cref="Instantiate()"/>
		public Component Instantiate(Vector3 position, Transform parent)
		{
			Component instance = Instantiate();
			if (instance == null)
				return instance;
			Transform trans = instance.transform;
			trans.parent = parent;
			trans.position = position;
			return instance;
		}

		/// <inheritdoc cref="Instantiate()"/>
		public Component Instantiate(Vector3 position, Quaternion rotation, Transform parent)
		{
			Component instance = Instantiate();
			if (instance == null)
				return instance;
			Transform trans = instance.transform;
			trans.parent = parent;
			trans.position = position;
			trans.rotation = rotation;
			return instance;
		}

		/// <inheritdoc cref="Instantiate()"/>
		public T Instantiate<T>() where T: Component
		{
			Component instance = Instantiate();
			return GetInstanceComponent<T>(instance);
		}

		/// <inheritdoc cref="Instantiate()"/>
		public T Instantiate<T>(Transform parent, bool worldSpace = false) where T: Component
		{
			Component instance = Instantiate(parent, worldSpace);
			return GetInstanceComponent<T>(instance);
		}

		/// <inheritdoc cref="Instantiate()"/>
		public T Instantiate<T>(Vector3 position) where T: Component
		{
			Component instance = Instantiate(position);
			return GetInstanceComponent<T>(instance);
		}

		/// <inheritdoc cref="Instantiate()"/>
		public T Instantiate<T>(Vector3 position, Quaternion rotation) where T: Component
		{
			Component instance = Instantiate(position, rotation);
			return GetInstanceComponent<T>(instance);
		}

		/// <inheritdoc cref="Instantiate()"/>
		public T Instantiate<T>(Vector3 position, Transform parent) where T: Component
		{
			Component instance = Instantiate(position, parent);
			return GetInstanceComponent<T>(instance);
		}

		/// <inheritdoc cref="Instantiate()"/>
		public T Instantiate<T>(Vector3 position, Quaternion rotation, Transform parent) where T: Component
		{
			Component instance = Instantiate(position, rotation, parent);
			return GetInstanceComponent<T>(instance);
		}

		private static T GetInstanceComponent<T>(Component instance)
		{
			if (instance is T component)
				return component;
			return instance.GetComponent<T>();
		}

		/// <summary>Pool a particular instance.</summary>
		public void Destroy(Component instance)
		{
			Used.Remove(instance);
			if (Limit && Overlimit && LimitMode == PoolLimitMode.DestroyAfterUse)
			{
				instance.gameObject.Destroy();
				return;
			}

			Pooler.GetInstanceInfo(instance).IsPooled = true;
			Available.AddLast(instance);
			SendDestroyMessage(instance);
			instance.gameObject.SetActive(false);
		}

		/// <summary>Pool all instances.</summary>
		public void DestroyAll()
		{
			foreach (Component instance in Used.Reverse())
				Destroy(instance);
		}

		#endregion

		#region Helper methods

		protected void SendInstantiateMessage(Component instance)
		{
			switch (MessageMode)
			{
				case PoolMessageMode.IPooledComponent:
					if (instance is IPooled pooled)
						pooled.AwakeFromPool();
					break;

				case PoolMessageMode.IPooledGameObject:
					instance.GetComponents<IPooled>().ForEach(p => p.AwakeFromPool());
					break;

				case PoolMessageMode.IPooledHierarchy:
					instance.GetComponentsInChildren<IPooled>().ForEach(p => p.AwakeFromPool());
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
				case PoolMessageMode.IPooledComponent:
					if (instance is IPooled pooled)
						pooled.OnDestroyIntoPool();
					break;

				case PoolMessageMode.IPooledGameObject:
					instance.GetComponents<IPooled>().ForEach(p => p.OnDestroyIntoPool());
					break;

				case PoolMessageMode.IPooledHierarchy:
					instance.GetComponentsInChildren<IPooled>().ForEach(p => p.OnDestroyIntoPool());
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
			return Used.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Used.GetEnumerator();
		}

		protected bool Overlimit => UsedCount >= LimitAmount;

		#endregion

		#region Editor functionality

#if UNITY_EDITOR
		// private void OnGroupChanged()
		// {
		// 	if (Group == null)
		// 		return;
		// 	if (!Group.Pools.Contains(this))
		// 		Group.Pools.Add(this);
		// }

		private IEnumerable<Component> GetPrefabComponents()
		{
			return Prefab != null ? Prefab.gameObject.GetComponents<Component>() : Enumerable.Empty<Component>();
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

		private bool ShowPersistent => base.transform.parent == null;
		private int MaxPreloadAmount => Limit ? LimitAmount : MaxPreloadLimit;
#endif

		#endregion

		#region Public properties

		/// <summary>Number of instances that can be re-used.</summary>
		public int AvailableCount => Available.Count;

		/// <summary>Number of instances that are in use.</summary>
		public int UsedCount => Used.Count;

		#endregion
	}
}