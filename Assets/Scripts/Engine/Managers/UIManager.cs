using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum WindowState
{
	Shown,
	Showing,
	Hidden,
	Hiding
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

	public static Window Show(string path,						
							object data = null,
							Transform parent = null,
							Action onShown = null,
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
						previous.Hide();
						break;

					case WindowConflictMode.OverwriteData:
						previous.Reshow(data, onShown);
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

		instance.Show(data, onShown, animation);
		instance.MarkAsInstance();

		return instance;
	}

	public static bool Hide(string name,
						Action onHidden = null,
						WindowHideMode mode = DefaultWindowHideMode,
						string animation = DefaultHideAnimation)
	{
		Window instance = Find(name);
		if (instance != null)
			return instance.Hide(onHidden, mode, animation);
		return false;
	}

	public static void Play(Transform from, AudioClip clip)
	{
		Transform root = from.root;
		AudioSource source = root.GetComponent<AudioSource>();
		if (source == null)
			source = root.gameObject.AddComponent<AudioSource>();
		source.PlayOneShot(clip);
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