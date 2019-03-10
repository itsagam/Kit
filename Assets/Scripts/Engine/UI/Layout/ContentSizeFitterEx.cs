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

	protected RectTransform cachedRectTransform;

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
			if (cachedRectTransform == null)
				cachedRectTransform = GetComponent<RectTransform>();
			return cachedRectTransform;
		}
	}
}