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
		Window,
		Name
	}

	public ShowHideMode Mode;
	public ShowHideType Type;
	public Window Window;
	public string Name;

	public void OnPointerClick (PointerEventData eventData)
	{
		switch (Type)
		{
			case ShowHideType.Window:
				if (Window != null)
				{
					switch (Mode)
					{
						case ShowHideMode.Show:
							Window.Show();
							break;

						case ShowHideMode.Hide:
							Window.Hide();
							break;

						case ShowHideMode.Toggle:
							if (Window.IsShown)
								Window.Hide();
							else
								Window.Show();
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
							if (!UIManager.IsShown(Name))
								UIManager.Show(Name);
							break;

						case ShowHideMode.Hide:
							if (UIManager.IsShown(Name))
								UIManager.Hide(Name);
							break;

						case ShowHideMode.Toggle:
							if (UIManager.IsShown(Name))
								UIManager.Hide(Name);
							else
								UIManager.Show(Name);
							break;
					}
				}
				break;
		}
	}
}