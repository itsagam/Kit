using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CloseButton : MonoBehaviour, IPointerClickHandler
{
	protected Button button;

	protected void Awake()
	{
		button = GetComponent<Button>();
		if (button != null)
			button.onClick.AddListener(Close);
	}

	public void OnPointerClick (PointerEventData eventData)
	{
		if (button == null)
			Close();
	}

	protected void Close()
	{
		UIManager.LastWindow?.Hide();
	}
}