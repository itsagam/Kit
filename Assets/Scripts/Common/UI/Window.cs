using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window : Popup
{
	public const string WindowsPath = Path + "Windows/";

    public static List<Window> Opened = new List<Window>();
    public static event Action<Window> OnWindowShowing;
	public static event Action<Window> OnWindowShown;
	public static event Action<Window> OnWindowHiding;
    public static event Action<Window> OnWindowHidden;

	public static new Window Show(string id, object data = null, Action onShown = null, PopupShowMode mode = PopupShowMode.HidePrevious, string animation = ShowAnimation)
	{
		return Show(id, GameObject.FindWithTag(Tag), data, onShown, mode, animation);
	}

	public static new Window Show(string id, GameObject parent, object data = null, Action onShown = null, PopupShowMode mode = PopupShowMode.HidePrevious, string animation =  ShowAnimation)
	{
		return Show(id, parent?.transform, data, onShown, mode, animation);
	}

	public static new Window Show(string id, Transform parent, object data = null, Action onShown = null, PopupShowMode mode = PopupShowMode.HidePrevious, string animation = ShowAnimation)
	{
		return (Window) Show(id, WindowsPath, parent, data, onShown, mode, animation);
	}

	public static new bool Hide(string id, Action onHidden = null, PopupHideMode mode = PopupHideMode.Auto, string animation = HideAnimation)
    {
		Window instance = Find(id);
        if (instance != null)
            return instance.Hide(onHidden, mode, animation);
        return false;
    }

    public static new bool IsShown(string id)
    {
        return Find(id) != null;
    }

	public static new Window Find(string id)
	{
		Window window = Opened.Find(w => w.name == id);
		if (window != null)
			return window;
		return null;
	}

	protected override void OnShowing()
	{
		base.OnShowing();
		Opened.Add(this);
		OnWindowShowing?.Invoke(this);
	}

	protected override void OnHiding()
	{
		base.OnHiding();
		OnWindowHiding?.Invoke(this);
	}

    protected override void OnShown()
    {
        base.OnShown();
		OnWindowShown?.Invoke(this);
	}

    protected override void OnHidden()
    {
        base.OnHidden();
		OnWindowHidden?.Invoke(this);
		Opened.Remove(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Opened.Remove(this);
	}

    public static Window Active
    {
        get
        {
            if (Opened.Count > 0)
                return Opened[Opened.Count - 1];
            return null;
        }
    }
}