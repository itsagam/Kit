using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Async;

public class TimedPopup : Window
{
	public float Time = 3.0f;

	protected IDisposable observable;

	protected override void OnShown()
	{
		QueueHide();
	}

	protected void QueueHide()
	{
		observable?.Dispose();
		observable = Observable.Timer(TimeSpan.FromSeconds(Time)).Subscribe(t => {
			observable = null;
			Hide().Forget();
		});
	}
}