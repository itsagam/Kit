using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedPopup : Popup
{
	public float Time = 3.0f;

	public override void Reshow(object data, Action onShown)
	{
		base.Reshow(data, onShown);
		CancelInvoke("HidePopup");
		Invoke("HidePopup", Time);
	}

	protected override void OnShown()
	{
		base.OnShown();
		Invoke("HidePopup", Time);
	}

	private void HidePopup()
	{
		Hide();
	}
}