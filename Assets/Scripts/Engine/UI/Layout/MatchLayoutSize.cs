using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Give Element the size of this transform whenever it's resized
public class MatchLayoutSize : MonoBehaviour
{
	public RectTransform Element;
	public Vector2 Padding;

	public bool Width = true;
	public bool Height = true;

	protected RectTransform rectTransform;

	protected void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
	}

	protected void OnRectTransformDimensionsChange()
	{
		if (Element && rectTransform)
		{
			Vector2 newSize = Element.sizeDelta;
			if (Width)
				newSize.x = rectTransform.sizeDelta.x + Padding.x;
			if (Height)
				newSize.y = rectTransform.sizeDelta.y + Padding.y;
			Element.sizeDelta = newSize;
		}
	}
}