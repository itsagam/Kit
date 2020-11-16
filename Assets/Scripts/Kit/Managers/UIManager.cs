using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Kit.UI;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Kit
{
	/// <summary>State of a window.</summary>
	public enum WindowState
	{
		/// <summary>The window is animating to be shown.</summary>
		Showing,

		/// <summary>The window has been shown.</summary>
		Shown,

		/// <summary>The window is animating to be hidden.</summary>
		Hiding,

		/// <summary>The window is hidden.</summary>
		Hidden
	}

	/// <summary>The action to take when a window with the same name already exists.</summary>
	public enum WindowConflictMode
	{
		/// <summary>Keep the previous window and show the new one as well.</summary>
		ShowNew,

		/// <summary>Just keep the previous window.</summary>
		DontShow,

		/// <summary>Overwrite and show the data on the previous window instead.</summary>
		OverwriteData,

		/// <summary>Hide the previous window (animations and all) and show the new one.</summary>
		HidePrevious
	}

	/// <summary>How to hide the window?</summary>
	public enum WindowHideMode
	{
		/// <summary>Decide automatically. Destroys if created using a prefab and de-activates if part of the scene.</summary>
		Auto,

		/// <summary>Deactivate the window <see cref="GameObject" />.</summary>
		Deactivate,

		/// <summary>Destroy the window <see cref="GameObject" />.</summary>
		Destroy
	}

	/// <summary>
	///     <para>
	///         Global access to UI and window management. Your screens need to derive from <see cref="Window" /> which you can then
	///         show by calling <see cref="UIManager.Show(Window, object, Transform, string, WindowConflictMode)" /> on prefabs.
	///     </para>
	///     <para>
	///         You can you also call <see cref="Window.Show(object)" />/<see cref="Window.Hide(WindowHideMode)" /> on them directly if
	///         they are in the scene, for example.
	///     </para>
	/// </summary>
	public static class UIManager
	{
		/// <summary>Default conflict mode to use in calls.</summary>
		public const WindowConflictMode DefaultConflictMode = WindowConflictMode.ShowNew;

		/// <summary>Default hide mode to use in calls.</summary>
		public const WindowHideMode DefaultHideMode = WindowHideMode.Auto;

		/// <summary>The sort order of the <see cref="Canvas" /> that's used for newly-created windows.</summary>
		public const int WindowCanvasOrder = 1000;

		/// <summary>List of all shown/showing windows.</summary>
		public static readonly List<Window> Windows = new List<Window>();

		/// <summary>Event that's called when any window is showing.</summary>
		public static event Action<Window> Showing;

		/// <summary>Event that's called when any window is shown.</summary>
		public static event Action<Window> Shown;

		/// <summary>Event that's called when any window is hiding.</summary>
		public static event Action<Window> Hiding;

		/// <summary>Event that's called when any window is hidden.</summary>
		public static event Action<Window> Hidden;

		private static Canvas lastCanvas = null;

		/// <summary>
		///     <para>Show a window from a path.</para>
		///     <para>Can be <c>await</c>-ed upon.</para>
		/// </summary>
		/// <param name="path">The path to window (should be in a Resources folder).</param>
		/// <param name="data">The data to pass to the window.</param>
		/// <param name="parent">The parent transform to attach the window to. Uses a general Canvas and transform by default.</param>
		/// <param name="animation">The animation state name to play when showing.</param>
		/// <param name="conflictMode">What to do if the window already exists?</param>
		/// <returns>An instance of the window.</returns>
		public static UniTask<Window> Show(string path,
										   object data = null,
										   Transform parent = default,
										   string animation = default,
										   WindowConflictMode conflictMode = DefaultConflictMode)
		{
			return ShowInternal(path, data, parent, animation, conflictMode);
		}

		/// <summary>
		///     <para>Show a window using a prefab.</para>
		///     <para>Can be <c>await</c>-ed upon.</para>
		/// </summary>
		/// <param name="prefab">The prefab to use for instantiating.</param>
		/// <param name="data">The data to pass to the window.</param>
		/// <param name="parent">The parent transform to attach the window to. Uses a general Canvas and transform by default.</param>
		/// <param name="animation">The animation state to play when showing.</param>
		/// <param name="conflictMode">What to do if the window already exists?</param>
		/// <returns>An instance of the window.</returns>
		public static UniTask<Window> Show(Window prefab,
										   object data = null,
										   Transform parent = default,
										   string animation = default,
										   WindowConflictMode conflictMode = DefaultConflictMode)
		{
			return ShowInternal(prefab, data, parent, animation, conflictMode);
		}

		// Workaround for CS4014: If you call async methods, but not await them, C# warns that you should.
		// Wrapping them in non-async methods prevents the warning.
		private static async UniTask<Window> ShowInternal(string path,
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

		private static async UniTask<Window> ShowInternal(Window prefab,
														  object data,
														  Transform parent,
														  string animation,
														  WindowConflictMode conflictMode)
		{
			if (conflictMode != WindowConflictMode.ShowNew)
			{
				Window previous = Find(prefab.name);
				if (previous != null)
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

		/// <summary>
		///     <para>Hide a window.</para>
		///     <para>Can be <c>await</c>-ed upon.</para>
		/// </summary>
		/// <param name="name">The window (prefab/<see cref="GameObject" />) name to hide.</param>
		/// <param name="animation">The animation state to play when hiding.</param>
		/// <param name="mode">Whether to de-activate or destroy the instance.</param>
		/// <returns>Whether the said window existed was successfully hidden.</returns>
		public static UniTask<bool> Hide(string name,
										 string animation = default,
										 WindowHideMode mode = DefaultHideMode)
		{
			Window window = Find(name);
			if (window != null)
				return animation != null ? window.Hide(animation, mode) : window.Hide(mode);
			return UniTask.FromResult(false);
		}

		/// <summary>Find a shown window by providing a name.</summary>
		/// <param name="name">The window (prefab/<see cref="GameObject" />) name.</param>
		/// <returns>Reference to the window.</returns>
		public static Window Find(string name)
		{
			return Windows.Find(w => w.name == name);
		}

		/// <summary>Find the first shown window of a given class.</summary>
		/// <returns>Reference to the window.</returns>
		public static T Find<T>() where T: Window
		{
			return Windows.OfType<T>().FirstOrDefault();
		}

		/// <summary>Returns whether a window with a particular name is shown.</summary>
		/// <param name="name">The window (prefab/<see cref="GameObject" />) name.</param>
		public static bool IsShown(string name)
		{
			return Find(name) != null;
		}

		/// <summary>Returns whether a window of a particular type is shown.</summary>
		public static bool IsShown<T>() where T: Window
		{
			return Find<T>() != null;
		}

		/// <summary>Register a window in the system. Called automatically.</summary>
		public static void Register(Window instance)
		{
			instance.Hidden.AddListener(() => Showing?.Invoke(instance));
			instance.Shown.AddListener(() => Shown?.Invoke(instance));
			instance.Shown.AddListener(() => Hiding?.Invoke(instance));
			instance.Shown.AddListener(() => Hidden?.Invoke(instance));
		}

		private static Canvas CreateCanvas()
		{
			GameObject uiGO = new GameObject("Windows") { layer = LayerMask.NameToLayer("UI") };
			Canvas canvas = uiGO.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.sortingOrder = WindowCanvasOrder;

			uiGO.AddComponent<CanvasScaler>();
			uiGO.AddComponent<GraphicRaycaster>();

			return canvas;
		}

		/// <summary>Returns the first window shown, or <see langword="null" /> if none are.</summary>
		public static Window First => Windows.FirstOrDefault();

		/// <summary>Returns the last window shown, or <see langword="null" /> if none are.</summary>
		public static Window Last => Windows.LastOrDefault();
	}
}