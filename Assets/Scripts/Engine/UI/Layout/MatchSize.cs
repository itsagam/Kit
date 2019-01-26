﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Give Element the size of this transform whenever it's resized
public class MatchSize : MonoBehaviour
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
			Vector2 newSize;
			newSize.x = Width ? rectTransform.sizeDelta.x + Padding.x : Element.sizeDelta.x;
			newSize.y = Height ? rectTransform.sizeDelta.y + Padding.y: Element.sizeDelta.y;
			Element.sizeDelta = newSize;
		}
	}
}