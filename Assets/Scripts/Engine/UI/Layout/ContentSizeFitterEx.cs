using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContentSizeFitterEx : ContentSizeFitter
{
	public Vector2 Padding;

	protected RectTransform rectTrans;

	public override void SetLayoutHorizontal()
	{
		base.SetLayoutVertical();
		rectTransform.sizeDelta = rectTransform.sizeDelta.AddY(Padding.x);
	}

	public override void SetLayoutVertical()
	{
		base.SetLayoutVertical();
		rectTransform.sizeDelta = rectTransform.sizeDelta.AddY(Padding.y);
	}

	protected RectTransform rectTransform
	{
		get
		{
			if (rectTrans == null)
				rectTrans = GetComponent<RectTransform>();
			return rectTrans;
		}
	}
}