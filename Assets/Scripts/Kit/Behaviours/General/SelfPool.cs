using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Kit.Pooling;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kit.Behaviours
{
	/// <summary>Pools the <see cref="GameObject" /> after a specified time.</summary>
	public class SelfPool: MonoBehaviour, IPooled
	{
		/// <summary>Time to hold out for in seconds before pooling.</summary>
		[Tooltip("Time to hold out for before pooling.")]
		[SuffixLabel("seconds", true)]
		public float Time = 5.0f;

		protected CancellationTokenSource cancelSource;

		public virtual void AwakeFromPool()
		{
			cancelSource = new CancellationTokenSource();
			UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(Time)).ForEachAsync(_ => Pool(), cancelSource.Token);
		}

		public virtual void OnDestroyIntoPool()
		{
			Cancel();
		}

		protected virtual void OnDestroy()
		{
			Cancel();
		}

		protected virtual void Cancel()
		{
			if (cancelSource != null)
			{
				cancelSource.Cancel();
				cancelSource.Dispose();
				cancelSource = null;
			}
		}

		public virtual void Pool()
		{
			Pooler.Destroy(this);
		}
	}
}