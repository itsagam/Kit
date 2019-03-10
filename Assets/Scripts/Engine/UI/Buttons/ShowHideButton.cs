using UnityEngine;
using UnityEngine.EventSystems;
using UniRx.Async;
using Sirenix.OdinInspector;

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

	[ShowIf("Type", ShowHideType.Window)]
	public Window Window;

	[ShowIf("Type", ShowHideType.Name)]
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
								UIManager.ShowWindow(Name);
							break;

						case ShowHideMode.Hide:
							if (UIManager.IsShown(Name))
								UIManager.HideWindow(Name);
							break;

						case ShowHideMode.Toggle:
							if (UIManager.IsShown(Name))
								UIManager.HideWindow(Name);
							else
								UIManager.ShowWindow(Name);
							break;
					}
				}
				break;
		}
	}
}