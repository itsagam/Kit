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

public class UIManager
{
	public const string DefaultShowAnimation = "Show";
	public const string DefaultHideAnimation = "Hide";
	public const WindowConflictMode DefaultConflictMode = WindowConflictMode.ShowNew;
	public const WindowHideMode DefaultWindowHideMode = WindowHideMode.Auto;

	public static List<Window> Windows = new List<Window>();

	public static event Action<Window> OnWindowShowing;
	public static event Action<Window> OnWindowShown;
	public static event Action<Window> OnWindowHiding;
	public static event Action<Window> OnWindowHidden;

	protected static AudioSource audio;

	public static async UniTask<Window> ShowWindow(
								string path,
								object data = null,
								Transform parent = null,
								WindowConflictMode mode = DefaultConflictMode,
								string animation = DefaultShowAnimation)
	{
		Window prefab = await ResourceManager.LoadAsync<Window>(ResourceFolder.Resources, path);
		if (prefab != null)
			return await ShowWindow(prefab, data, parent, mode, animation);
		else
			return null;
	}

	public static async UniTask<Window> ShowWindow(
							Window prefab,
							object data = null,
							Transform parent = null,
							WindowConflictMode mode = DefaultConflictMode,
							string animation = DefaultShowAnimation)
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
			parent = GameObject.FindObjectOfType<Canvas>()?.transform;
			if (parent == null)
				parent = CreateCanvas().transform;
		}
		instance.transform.SetParent(parent, false);

		await instance.Show(data, animation);

		return instance;
	}

	public static UniTask<bool> HideWindow(
						string name,
						WindowHideMode mode = DefaultWindowHideMode,
						string animation = DefaultHideAnimation)
	{
		return HideWindow(FindWindow(name), mode, animation);
	}

	public static UniTask<bool> HideWindow(
					Window window,
					WindowHideMode mode = DefaultWindowHideMode,
					string animation = DefaultHideAnimation)
	{
		if (window != null)
			return window.Hide(mode, animation);	
		else
			return UniTask.FromResult(false);
	}

	public static void Play(Transform from, AudioClip clip)
	{
		if (clip != null)
			Audio.PlayOneShot(clip);
	}

	public static Window FindWindow(string name)
	{
		return Windows.Find(w => w.name == name);
	}

	public static bool IsShown(string name)
	{
		return FindWindow(name) != null;
	}

	public static void RegisterWindow(Window instance)
	{
		instance.OnWindowShowing += () => OnWindowShowing?.Invoke(instance);
		instance.OnWindowShown += () => OnWindowShown?.Invoke(instance);
		instance.OnWindowHiding += () => OnWindowHiding?.Invoke(instance);
		instance.OnWindowHidden += () => OnWindowHidden?.Invoke(instance);
	}

	protected static Canvas CreateCanvas()
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

	public static AudioSource Audio
	{
		get
		{
			if (audio == null)
			{
				GameObject audioGO = new GameObject("UIAudio");
				audio = audioGO.AddComponent<AudioSource>();
				audio.spatialBlend = 0;
				GameObject.DontDestroyOnLoad(audioGO);
			}
			return audio;
		}
	}
}