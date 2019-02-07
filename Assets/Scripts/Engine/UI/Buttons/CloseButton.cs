using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CloseButton : MonoBehaviour, IPointerClickHandler
{
	public void OnPointerClick (PointerEventData eventData)
	{
		UIManager.Last?.Hide();
	}
}