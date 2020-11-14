using System;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Engine.UI
{
	/// <summary>
	/// A <see cref="Window"/> that hides itself automatically after a specified time.
	/// </summary>
	public class TimedWindow: Window
	{
		/// <summary>
		/// Duration to display the window for in seconds.
		/// </summary>
		[PropertyOrder(-99)]
		[SuffixLabel("seconds", true)]
		[MinValue(0)]
		[Tooltip("Duration to display the window for.")]
		public float Time = 3.0f;

		protected IDisposable observable;

		protected override void OnShown()
		{
			QueueHide();
		}

		protected virtual void QueueHide()
		{
			observable?.Dispose();
			observable = Observable.Timer(TimeSpan.FromSeconds(Time))
								   .Subscribe(t =>
											  {
												  observable = null;
												  Hide();
											  });
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			observable?.Dispose();
		}
	}
}