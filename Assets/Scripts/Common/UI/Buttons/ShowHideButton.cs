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
		Name
	}

	public ShowHideMode Mode;
	public ShowHideType Type;
	public Popup Popup;
	public string Name;

	public void OnPointerClick (PointerEventData eventData)
	{
		switch (Type)
		{
			case ShowHideType.Popup:
				if (Popup != null)
				{
					switch (Mode)
					{
						case ShowHideMode.Show:
							Popup.Show();
							break;

						case ShowHideMode.Hide:
							Popup.Hide();
							break;

						case ShowHideMode.Toggle:
							if (Popup.IsShown())
								Popup.Hide();
							else
								Popup.Show();
							break;
					}
				}
				break;

			case ShowHideType.Name:
				if (!Name.IsNullOrEmpty())
				{
					switch (Mode)
					{
						case ShowHideMode.Show:
							if (!Popup.IsShown(Name))
								Popup.Show(Name);
							break;

						case ShowHideMode.Hide:
							if (Popup.IsShown(Name))
								Popup.Hide(Name);
							break;

						case ShowHideMode.Toggle:
							if (Popup.IsShown(Name))
								Popup.Hide(Name);
							else
								Popup.Show(Name);
							break;
					}
				}
				break;
		}
	}
}