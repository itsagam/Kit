using System;
using DG.Tweening;
using Engine;
using UniRx.Async;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public enum FadeMode
{
    None,
    FadeOut,
    FadeIn,
    FadeOutIn
}

public static class SceneDirector
{
	public static event Action<string> OnSceneChanging;
	public static event Action<string> OnSceneChanged;
	public static event Action OnFadingIn;
	public static event Action OnFadedIn;
	public static event Action OnFadingOut;
	public static event Action OnFadedOut;
	public static event Action<float> OnFading;
	public static event Action<float> OnFaded;

	public const FadeMode DefaultFadeMode = FadeMode.FadeOutIn;
	public static readonly Color DefaultFadeColor = Color.black;
	public const float DefaultFadeTime = 1.0f;

	private static Image fadeImage;

	public static UniTask Fade(float to, float? from = null,
							   Color? color = null, float time = DefaultFadeTime,
							   Action onComplete = null)
	{
		if (fadeImage == null)
			fadeImage = CreateFadeImage();

		OnFading?.Invoke(to);
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
							  OnFaded?.Invoke(to);
						  })
			  .ToUniTask();
	}

	public static UniTask FadeIn(Color? color = null, float time = DefaultFadeTime, Action onComplete = null)
	{
		OnFadingIn?.Invoke();
		return Fade(1,
					0,
					color,
					time,
					() =>
					{
						onComplete?.Invoke();
						OnFadedIn?.Invoke();
					});
	}

	public static UniTask FadeOut(Color? color = null, float time = DefaultFadeTime, Action onComplete = null)
	{
		OnFadingOut?.Invoke();
		return Fade(0,
					1,
					color,
					time,
					() =>
					{
						onComplete?.Invoke();
						OnFadedOut?.Invoke();
					});
	}

	public static UniTask ReloadScene(FadeMode fadeMode = DefaultFadeMode,
									  Color? fadeColor = null, float fadeTime = DefaultFadeTime,
									  bool additive = false,
									  Action<float> onLoadProgress = null, Action onLoadComplete = null,
									  Action onComplete = null)
	{
		return LoadScene(ActiveScene.path, fadeMode, fadeColor, fadeTime, additive, onLoadProgress, onLoadComplete, onComplete);
	}

	public static async UniTask LoadScene(string nameOrPath,
										  FadeMode fadeMode = DefaultFadeMode,
										  Color? fadeColor = null, float fadeTime = DefaultFadeTime,
										  bool additive = false,
										  Action<float> onLoadProgress = null, Action onLoadComplete = null,
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
										   Action<float> onProgress = null, Action onComplete = null)
	{
		if (!additive)
			OnSceneChanging?.Invoke(nameOrPath);

		AsyncOperation load = SceneManager.LoadSceneAsync(nameOrPath, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
		if (onProgress != null)
			await load.ConfigureAwait(new Progress<float>(onProgress));
		else
			await load;
		onComplete?.Invoke();
		if (!additive)
			OnSceneChanged?.Invoke(nameOrPath);
	}

	private static Image CreateFadeImage()
    {
		GameObject gameObject = new GameObject(typeof(SceneManager).Name);
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
		Image image = gameObject.AddComponent<Image>();
		Object.DontDestroyOnLoad(gameObject);
        return image;
    }

	public static Scene ActiveScene => SceneManager.GetActiveScene();
	public static string ActiveName => ActiveScene.name;

	public static bool IsScene(string name)
	{
		return ActiveScene.name == name;
	}
}