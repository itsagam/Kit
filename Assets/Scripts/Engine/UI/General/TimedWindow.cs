using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class TimedWindow : Window
{
	public float Time = 3.0f;

	protected IDisposable observable;

	public override void Reshow(object data, Action onShown)
	{
		base.Reshow(data, onShown);
		QueueHide();
	}

	protected override void OnShown()
	{
		base.OnShown();
		QueueHide();
	}

	protected void QueueHide()
	{
		observable?.Dispose();
		observable = Observable.Timer(TimeSpan.FromSeconds(Time)).Subscribe(t => {
			observable = null;
			Hide();
		});
	}
}