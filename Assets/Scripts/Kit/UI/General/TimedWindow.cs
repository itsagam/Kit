using System;
using System.Threading;
using Cysharp.Threading.Tasks.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kit.UI
{
	/// <summary>A <see cref="Window" /> that hides itself automatically after a specified time.</summary>
	public class TimedWindow: Window
	{
		/// <summary>Duration to display the window for in seconds.</summary>
		[PropertyOrder(-99)]
		[SuffixLabel("seconds", true)]
		[MinValue(0)]
		[Tooltip("Duration to display the window for.")]
		public float Time = 3.0f;

		protected CancellationTokenSource cancelSource;

		protected override void OnShown()
		{
			QueueHide();
		}

		protected override void OnHidden()
		{
			Cancel();
		}

		protected virtual void QueueHide()
		{
			Cancel();
			cancelSource = new CancellationTokenSource();
			UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(Time))
								  .ForEachAsync(_ =>
												{
													Hide();
												}, cancelSource.Token);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
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
	}
}