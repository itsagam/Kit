using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayOnClick : MonoBehaviour, IPointerDownHandler
{
	public AudioClip Audio;

	public void OnPointerDown (PointerEventData eventData)
	{
		Button button = GetComponent<Button>();
		if (button == null || button.IsInteractable())
			Popup.Play(button.transform, Audio);
	}
}