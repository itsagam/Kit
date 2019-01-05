using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShowHideButton : MonoBehaviour, IPointerClickHandler
{
	public enum ShowHideMode
	{
		Show,
		Hide,
		Toggle
	}

	public enum ShowHideType
	{
		Popup,
		ID
	}

	public ShowHideMode Mode;
	public ShowHideType Type;
	public Popup Popup;
	public string ID;

	public void OnPointerClick (PointerEventData eventData)
	{
		if (Type == ShowHideType.Popup)
		{
			if (Popup != null)
			{
				if (Mode == ShowHideMode.Show)
					Popup.Show();
				else if (Mode == ShowHideMode.Hide)
					Popup.Hide();
				else
				{
					if (Popup.IsShown())
						Popup.Hide();
					else
						Popup.Show();
				}
			}
		}
		else
		{
			if (!ID.IsNullOrEmpty())
			{
				if (Mode == ShowHideMode.Show)
				{
					if (! Popup.IsShown(ID))
						Popup.Show(ID);
				}
				else if (Mode == ShowHideMode.Hide)
				{
					if (Popup.IsShown(ID))					
						Popup.Hide(ID);
				}
				else
				{
					if (Popup.IsShown(ID))
						Popup.Hide(ID);
					else
						Popup.Show(ID);
				}
			}
		}
	}
}