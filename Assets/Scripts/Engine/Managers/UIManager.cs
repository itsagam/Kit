using System;
using System.Collections.Generic;
using System.Linq;
using Engine.UI;
using UniRx.Async;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Engine
{
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
		HidePrevious
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
		public const WindowHideMode DefaultHideMode = WindowHideMode.Auto;

		public static readonly List<Window> Windows = new List<Window>();

		public static event Action<Window> Showing;
		public static event Action<Window> Shown;
		public static event Action<Window> Hiding;
		public static event Action<Window> Hidden;

		private static Canvas lastCanvas = null;

		// Workaround for CS4014: If call async methods, but not await them, C# warns that you should.
		// Wrapping them in non-async methods prevents the warning.
		public static UniTask<Window> Show(
			string path,
			object data = null,
			Transform parent = default,
			string animation = default,
			WindowConflictMode conflictMode = DefaultConflictMode)
		{
			return ShowInternal(path, data, parent, animation, conflictMode);
		}

		public static UniTask<Window> Show(
			Window prefab,
			object data = null,
			Transform parent = default,
			string animation = default,
			WindowConflictMode conflictMode = DefaultConflictMode)
		{
			return ShowInternal(prefab, data, parent, animation, conflictMode);
		}

		private static async UniTask<Window> ShowInternal(
			string path,
			object data,
			Transform parent,
			string animation,
			WindowConflictMode conflictMode)
		{
			Window prefab = await ResourceManager.LoadAsync<Window>(ResourceFolder.Resources, path);
			if (prefab == null)
				return null;

			return await Show(prefab, data, parent, animation, conflictMode);
		}

		private static async UniTask<Window> ShowInternal(
			Window prefab,
			object data,
			Transform parent,
			string animation,
			WindowConflictMode conflictMode)
		{

			if (conflictMode != WindowConflictMode.ShowNew)
			{
				Window previous = Find(prefab.name);
				if (previous != null)
				{
					switch (conflictMode)
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

			if (parent == null)
			{
				if (lastCanvas == null)
					lastCanvas = CreateCanvas();
				parent = lastCanvas.transform;
			}

			Window instance = Object.Instantiate(prefab, parent, false);
			instance.name = prefab.name;
			instance.MarkAsInstance();

			if (animation == null)
				await instance.Show(data);
			else
				await instance.Show(animation, data);

			return instance;
		}

		public static UniTask<bool> Hide(
			string name,
			string animation = default,
			WindowHideMode mode = DefaultHideMode)
		{
			Window window = Find(name);
			if (window != null)
				return animation != null ? window.Hide(animation, mode) : window.Hide(mode);
			return UniTask.FromResult(false);
		}

		public static Window Find(string name)
		{
			return Windows.Find(w => w.name == name);
		}

		public static T Find<T>() where T: Window
		{
			return Windows.OfType<T>().FirstOrDefault();
		}

		public static bool IsShown(string name)
		{
			return Find(name) != null;
		}

		public static bool IsShown<T>() where T: Window
		{
			return Find<T>() != null;
		}

		public static void Register(Window instance)
		{
			instance.Hidden.AddListener(() => Showing?.Invoke(instance));
			instance.Shown.AddListener(() => Shown?.Invoke(instance));
			instance.Shown.AddListener(() => Hiding?.Invoke(instance));
			instance.Shown.AddListener(() => Hidden?.Invoke(instance));
		}

		private static Canvas CreateCanvas()
		{
			GameObject uiGO = new GameObject("Windows") {layer = LayerMask.NameToLayer("UI")};
			Canvas canvas = uiGO.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;

			uiGO.AddComponent<CanvasScaler>();
			uiGO.AddComponent<GraphicRaycaster>();

			return canvas;
		}

		public static Window First => Windows.FirstOrDefault();
		public static Window Last => Windows.LastOrDefault();
	}
}