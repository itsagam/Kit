using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GotoButton : MonoBehaviour, IPointerClickHandler
{
	public Wizard Wizard;
	public Popup Step;

	public void OnPointerClick (PointerEventData eventData)
	{
		if (Wizard != null)
			Wizard.GoTo(Step);
	}
}