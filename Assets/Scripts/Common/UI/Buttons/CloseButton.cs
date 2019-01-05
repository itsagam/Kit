using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CloseButton : MonoBehaviour, IPointerClickHandler
{
	public void OnPointerClick (PointerEventData eventData)
	{
		Window active = Window.Active;
		if (active != null)
			active.Hide();
	}
}