using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
[CustomEditor(typeof(ContentSizeFitterEx), false)]
public class ContentSizeFitterExEditor : OdinEditor
{
}
#endif

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