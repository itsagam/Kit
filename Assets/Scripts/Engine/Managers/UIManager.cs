using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UniRx.Async;

public enum WindowState
{
	Showing,
	Shown,
	Hiding,
	Hidden
}

public enum WindowConflictMode
{
	ShowNew,
	DontShow,
	OverwriteData,
	HidePrevious,
}

public enum WindowHideMode
{
	Auto,
	Deactivate,
	Destroy
}

public static class UIManager
{
	public const WindowConflictMode DefaultConflictMode = WindowConflictMode.ShowNew;
	public const WindowHideMode DefaultWindowHideMode = WindowHideMode.Auto;

	public static List<Window> Windows = new List<Window>();

	public static event Action<Window> OnWindowShowing;
	public static event Action<Window> OnWindowShown;
	public static event Action<Window> OnWindowHiding;
	public static event Action<Window> OnWindowHidden;

	private static Canvas lastCanvas = null;

	public static async UniTask<Window> ShowWindow(
								string path,
								object data = null,
								Transform parent = null,
								string animation = null,
								WindowConflictMode mode = DefaultConflictMode)
	{
		Window prefab = await ResourceManager.LoadAsync<Window>(ResourceFolder.Resources, path);
		if (prefab != null)
			return await ShowWindow(prefab, data, parent, animation, mode);
		else
			return null;
	}

	public static async UniTask<Window> ShowWindow(
							Window prefab,
							object data = null,
							Transform parent = null,
							string animation = null,
							WindowConflictMode mode = DefaultConflictMode)
	{

		if (mode != WindowConflictMode.ShowNew)
		{
			Window previous = FindWindow(prefab.name);
			if (previous != null)
			{
				switch (mode)
				{
					case WindowConflictMode.DontShow:
						return null;

					case WindowConflictMode.HidePrevious:
						if (!await previous.Hide())
							return null;
						break;

					case WindowConflictMode.OverwriteData:
						previous.Data = data;
						return previous;
				}
			}
		}
		
		Window instance = GameObject.Instantiate(prefab);
		instance.name = prefab.name;
		instance.MarkAsInstance();

		if (parent == null)
		{
			if (lastCanvas == null)
			{
				lastCanvas = GameObject.FindObjectOfType<Canvas>();
				if (lastCanvas == null)
					lastCanvas = CreateCanvas();
			}
			parent = lastCanvas.transform;
		}
		instance.transform.SetParent(parent, false);

		if (animation == null)
			await instance.Show(data);
		else
			await instance.Show(animation, data);

		return instance;
	}

	public static UniTask<bool> HideWindow(
						string name,
						string animation = null,
						WindowHideMode mode = DefaultWindowHideMode)
	{
		return HideWindow(FindWindow(name), animation, mode);
	}

	public static UniTask<bool> HideWindow(
					Window window,
					string animation = null,
					WindowHideMode mode = DefaultWindowHideMode)
	{
		if (window != null)
			if (animation == null)
				return window.Hide(mode);
			else
				return window.Hide(animation, mode);
		else
			return UniTask.FromResult(false);
	}

	public static Window FindWindow(string name)
	{
		return Windows.Find(w => w.name == name);
	}

	public static T FindWindow<T>() where T: Window
	{
		return Windows.OfType<T>().FirstOrDefault();
	}

	public static bool IsShown(string name)
	{
		return FindWindow(name) != null;
	}

	public static bool IsShown<T>() where T: Window
	{
		return FindWindow<T>() != null;
	}

	public static void RegisterWindow(Window instance)
	{
		instance.OnWindowHidden.AddListener(() => OnWindowShowing?.Invoke(instance));
		instance.OnWindowShown.AddListener(() => OnWindowShown?.Invoke(instance));
		instance.OnWindowShown.AddListener(() => OnWindowHiding?.Invoke(instance));
		instance.OnWindowShown.AddListener(() => OnWindowHidden?.Invoke(instance));
	}

	private static Canvas CreateCanvas()
	{
		GameObject ui = new GameObject("UI");
		ui.layer = LayerMask.NameToLayer("UI");

		Canvas canvas = ui.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;

		ui.AddComponent<CanvasScaler>();
		ui.AddComponent<GraphicRaycaster>();

		return canvas;
	}

	public static Window FirstWindow
	{
		get
		{
			return Windows.FirstOrDefault();
		}
	}

	public static Window LastWindow
	{
		get
		{
			return Windows.LastOrDefault();
		}
	}
}