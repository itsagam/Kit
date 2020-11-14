using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Engine
{
	/// <summary>
	/// Should fade in, out, or both?
	/// </summary>
	public enum FadeMode
	{
		/// <summary>
		/// Do not fade.
		/// </summary>
		None,

		/// <summary>
		/// Fade out.
		/// </summary>
		FadeOut,

		/// <summary>
		/// Fade in.
		/// </summary>
		FadeIn,

		/// <summary>
		/// Fade out, then fade in.
		/// </summary>
		FadeOutIn
	}

	/// <summary>
	/// Allows to fade in/out scenes or load them with it. Provides hooks.
	/// </summary>
	public static class SceneDirector
	{
		/// <summary>
		/// Default fade mode to use in calls.
		/// </summary>
		public const FadeMode DefaultFadeMode = FadeMode.FadeOutIn;

		/// <summary>
		/// Default fade color to use in calls.
		/// </summary>
		public static readonly Color DefaultFadeColor = Color.black;

		/// <summary>
		/// Default fade time to use in calls.
		/// </summary>
		public const float DefaultFadeTime = 1.0f;

		/// <summary>
		/// The sort order of the <see cref="Canvas"/> that's used for fading.
		/// </summary>
		public const int FadeCanvasOrder = 9999;


		/// <summary>
		/// Event that gets called when the scene starts changing.
		/// </summary>
		public static event Action<string> SceneChanging;

		/// <summary>
		/// Event that gets called when the scene has changed.
		/// </summary>
		public static event Action<string> SceneChanged;

		/// <summary>
		/// Event that gets called when the scene starts fading in.
		/// </summary>
		public static event Action FadingIn;

		/// <summary>
		/// Event that gets called when the scene has faded in.
		/// </summary>
		public static event Action FadedIn;

		/// <summary>
		/// Event that gets called when the scene starts fading out.
		/// </summary>
		public static event Action FadingOut;

		/// <summary>
		/// Event that gets called when the scene has faded out.
		/// </summary>
		public static event Action FadedOut;

		/// <summary>
		/// Event that gets called when the scene is fading (in or out).
		/// </summary>
		public static event Action<float> Fading;

		/// <summary>
		/// Event that gets called when the scene has faded (in or out).
		/// </summary>
		public static event Action<float> Faded;

		private static Image fadeImage;

        /// <summary>
        /// <para>Fade the screen.</para>
        /// <para>Can be <c>await</c>-ed upon.</para>
        /// </summary>
        /// <param name="to">To alpha.</param>
        /// <param name="from">From alpha. Current if not specified.</param>
        /// <param name="color">Fade color. Default if not specified.</param>
        /// <param name="time">Time to take.</param>
        /// <param name="onComplete">Method to call when done.</param>
        /// <returns>A UniTask that emits when fading's done.</returns>
        public static UniTask Fade(float to,
								   float? from = null,
								   Color? color = null,
								   float time = DefaultFadeTime,
								   Action onComplete = null)
		{
			if (fadeImage == null)
				fadeImage = CreateFadeImage();

			Fading?.Invoke(to);
			fadeImage.gameObject.SetActive(true);
			fadeImage.DOKill();

			if (!color.HasValue)
				color = DefaultFadeColor;

			if (!from.HasValue)
				from = fadeImage.color.a;

			color = color.Value.SetAlpha(1 - from.Value);

			fadeImage.color = color.Value;
			return fadeImage
				  .DOFade(1 - to, time)
				  .OnComplete(() =>
							  {
								  if (fadeImage.color.a <= 0)
									  fadeImage.gameObject.SetActive(false);
								  onComplete?.Invoke();
								  Faded?.Invoke(to);
							  })
				  .ToUniTask();
		}

		/// <summary>
		/// <para>Fade in the screen.</para>
		/// <para>Can be <c>await</c>-ed upon.</para>
		/// </summary>
		/// <param name="color">Fade color. Default if not specified.</param>
		/// <param name="time">Time to take.</param>
		/// <param name="onComplete">Method to call when done.</param>
		/// <returns>A UniTask that emits when fading's done.</returns>
		public static UniTask FadeIn(Color? color = null, float time = DefaultFadeTime, Action onComplete = null)
		{
			FadingIn?.Invoke();
			return Fade(1,
						0,
						color,
						time,
						() =>
						{
							onComplete?.Invoke();
							FadedIn?.Invoke();
						});
		}

		/// <summary>
		/// <para>Fade out the screen.</para>
		/// <para>Can be <c>await</c>-ed upon.</para>
		/// </summary>
		/// <param name="color">Fade color. Default if not specified.</param>
		/// <param name="time">Time to take.</param>
		/// <param name="onComplete">Method to call when done.</param>
		/// <returns>A UniTask that emits when fading's done.</returns>
		public static UniTask FadeOut(Color? color = null, float time = DefaultFadeTime, Action onComplete = null)
		{
			FadingOut?.Invoke();
			return Fade(0,
						1,
						color,
						time,
						() =>
						{
							onComplete?.Invoke();
							FadedOut?.Invoke();
						});
		}

		/// <summary>
		/// <para>Reload the active scene.</para>
		/// <para>Can be <c>await</c>-ed upon.</para>
		/// </summary>
		/// <param name="fadeMode">How to fade?</param>
		/// <param name="fadeColor">Fade color. Default if not specified.</param>
		/// <param name="fadeTime">Time to take for fading.</param>
		/// <param name="additive">Whether to load the scene additively.</param>
		/// <param name="onLoadProgress">Method to call when loading progresses.</param>
		/// <param name="onLoadComplete">Method to call when loading completes.</param>
		/// <param name="onComplete">Method to call when loading and fading (if applicable) completes.</param>
		/// <returns>A UniTask that emits when fading's done.</returns>
		public static UniTask ReloadScene(FadeMode fadeMode = DefaultFadeMode,
										  Color? fadeColor = null,
										  float fadeTime = DefaultFadeTime,
										  bool additive = false,
										  Action<float> onLoadProgress = null,
										  Action onLoadComplete = null,
										  Action onComplete = null)
		{
			return LoadScene(ActiveScene.path, fadeMode, fadeColor, fadeTime, additive, onLoadProgress, onLoadComplete, onComplete);
		}

		/// <summary>
		/// <para>Load a scene.</para>
		/// <para>Can be <c>await</c>-ed upon.</para>
		/// </summary>
		/// <param name="nameOrPath">Name or path of the scene to load.</param>
		/// <param name="fadeMode">How to fade?</param>
		/// <param name="fadeColor">Fade color. Default if not specified.</param>
		/// <param name="fadeTime">Time to take for fading.</param>
		/// <param name="additive">Whether to load the scene additively.</param>
		/// <param name="onLoadProgress">Method to call when loading progresses.</param>
		/// <param name="onLoadComplete">Method to call when loading completes.</param>
		/// <param name="onComplete">Method to call when loading and fading (if applicable) completes.</param>
		/// <returns>A UniTask that emits when fading's done.</returns>
		public static async UniTask LoadScene(string nameOrPath,
											  FadeMode fadeMode = DefaultFadeMode,
											  Color? fadeColor = null,
											  float fadeTime = DefaultFadeTime,
											  bool additive = false,
											  Action<float> onLoadProgress = null,
											  Action onLoadComplete = null,
											  Action onComplete = null)
		{
			if (fadeMode == FadeMode.FadeOut || fadeMode == FadeMode.FadeOutIn)
				await FadeOut(fadeColor, fadeTime);

			await LoadScene(nameOrPath, additive, onLoadProgress, onLoadComplete);

			if (fadeMode == FadeMode.FadeOutIn)
				await FadeIn(fadeColor, fadeTime);

			onComplete?.Invoke();
		}

		private static async UniTask LoadScene(string nameOrPath,
											   bool additive = false,
											   Action<float> onProgress = null,
											   Action onComplete = null)
		{
			if (!additive)
				SceneChanging?.Invoke(nameOrPath);

			AsyncOperation load = SceneManager.LoadSceneAsync(nameOrPath, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
			if (onProgress != null)
				await load.ToUniTask(new Progress<float>(onProgress));
			else
				await load;
			onComplete?.Invoke();
			if (!additive)
				SceneChanged?.Invoke(nameOrPath);
		}

		private static Image CreateFadeImage()
		{
			GameObject gameObject = new GameObject(nameof(SceneManager));
			Canvas canvas = gameObject.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.sortingOrder = FadeCanvasOrder;
			Image image = gameObject.AddComponent<Image>();
			Object.DontDestroyOnLoad(gameObject);
			return image;
		}

		/// <summary>
		/// Reference to the active scene.
		/// </summary>
		public static Scene ActiveScene => SceneManager.GetActiveScene();

		/// <summary>
		/// Name of the active scene.
		/// </summary>
		public static string ActiveName => ActiveScene.name;

		/// <summary>
		/// Returns whether the active scene is a particular one.
		/// </summary>
		public static bool IsScene(string name)
		{
			return ActiveScene.name == name;
		}
	}
}