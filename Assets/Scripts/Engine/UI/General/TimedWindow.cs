using System;
using Sirenix.OdinInspector;
using UniRx;

namespace Engine.UI
{
	public class TimedWindow: Window
	{
		[PropertyOrder(-99)]
		[SuffixLabel("seconds", true)]
		[MinValue(0)]
		public float Time = 3.0f;

		protected IDisposable observable;

		protected override void OnShown()
		{
			QueueHide();
		}

		protected void QueueHide()
		{
			observable?.Dispose();
			observable = Observable.Timer(TimeSpan.FromSeconds(Time))
								   .Subscribe(t =>
											  {
												  observable = null;
												  Hide();
											  });
		}
	}
}