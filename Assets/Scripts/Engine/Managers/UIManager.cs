﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
	public const WindowConflictMode DefaultConflictMode = WindowConflictMode.HidePrevious;
	public const WindowHideMode DefaultWindowHideMode = WindowHideMode.Auto;

	public static List<Window> Shown = new List<Window>();

	public static event Action<Window> OnWindowShowing;
	public static event Action<Window> OnWindowShown;
	public static event Action<Window> OnWindowHiding;
	public static event Action<Window> OnWindowHidden;

	public static AudioSource Audio;

	public static async UniTask<Window> Show(
								string path,						
								object data = null,
								Transform parent = null,
								WindowConflictMode mode = DefaultConflictMode,
								string animation = DefaultShowAnimation)
	{
		string name = Path.GetFileName(path);
		if (mode != WindowConflictMode.ShowNew)
		{
			Window previous = Find(name);
			if (previous != null)
			{
				switch (mode)
				{
					case WindowConflictMode.DontShow:
						return null;

					case WindowConflictMode.HidePrevious:
						await previous.Hide();
						break;

					case WindowConflictMode.OverwriteData:
						previous.Reshow(data);
						return previous;
				}
			}
		}

		Window prefab = ResourceManager.Load<Window>(ResourceFolder.Resources, path);
		Window instance = UnityEngine.Object.Instantiate(prefab);
		instance.name = prefab.name;

		if (parent == null)
			parent = UnityEngine.Object.FindObjectOfType<Canvas>()?.transform;
		if (parent != null)
			instance.transform.SetParent(parent, false);

		await instance.Show(data, animation);
		
		return instance;
	}

	public static async UniTask<bool> Hide(
						string name,
						WindowHideMode mode = DefaultWindowHideMode,
						string animation = DefaultHideAnimation)
	{
		Window instance = Find(name);
		if (instance != null)
		{
			await instance.Hide(mode, animation);
			return true;
		}
		return false;
	}

	public static void InvokeEvent(WindowState state, Window window)
	{
		switch (state)
		{
			case WindowState.Showing:
				OnWindowShowing?.Invoke(window);
				break;
			case WindowState.Shown:
				OnWindowShown?.Invoke(window);
				break;
			case WindowState.Hiding:
				OnWindowHiding?.Invoke(window);
				break;
			case WindowState.Hidden:
				OnWindowHidden?.Invoke(window);
				break;
		}
	}

	public static void Play(Transform from, AudioClip clip)
	{
		if (Audio == null)
		{
			GameObject audioGO = new GameObject("UIAudio");
			Audio = audioGO.AddComponent<AudioSource>();
			UnityEngine.Object.DontDestroyOnLoad(audioGO);
		}
		Audio.PlayOneShot(clip);
	}

	public static Window Find(string name)
	{
		return Shown.Find(w => w.name == name);
	}

	public static bool IsShown(string name)
	{
		return Find(name) != null;
	}

	public static Window First
	{
		get
		{
			return Shown.FirstOrDefault();
		}
	}

	public static Window Last
	{
		get
		{
			return Shown.LastOrDefault();
		}
	}
}