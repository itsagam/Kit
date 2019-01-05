using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UniRx;
using UniRx.Async;

public enum FadeMode
{
    None,
    FadeOut,
    FadeIn,
    FadeOutIn
}

// TODO: Remove the need for Singleton
public class SceneHelper: Singleton<SceneHelper>
{
	public const string SplashScene = "Splash";
	public const string MainScene = "Main";

    public const string FadeName = "Fade";
    public const int FadeOrder = 9999;
    public const float FadeTime = 1.0f;
	public static Color FadeColor = Color.black;

	public static event Action<string> OnSceneChanging;
	public static event Action OnFadingIn;
	public static event Action OnFadedIn;
	public static event Action OnFadingOut;
	public static event Action OnFadedOut;
	public static event Action<float, float> OnFading;
	public static event Action<float, float> OnFaded;

	protected static Image fadeImage;

    protected void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

	public static void LoadMainScene(FadeMode fadeMode, Action onComplete = null, Action<float> onProgress = null, float time = FadeTime)
	{
		LoadScene(MainScene, fadeMode, onComplete, onProgress, time);
	}

	public static void ReloadScene(FadeMode fadeMode, Action onComplete = null, Action<float> onProgress = null, float time = FadeTime)
	{
		ReloadScene(fadeMode, FadeColor, onComplete, onProgress, time);
	}

	public static void ReloadScene(FadeMode fadeMode, Color fadeColor, Action onComplete = null, Action<float> onProgress = null, float time = FadeTime)
	{
		LoadScene(ActiveScene, fadeMode, fadeColor, onComplete, onProgress, time);
	}

	public static void LoadScene(string name, FadeMode fadeMode, Action onComplete = null, Action<float> onProgress = null, float time = FadeTime)
    {
		LoadScene(name, fadeMode, FadeColor, onComplete, onProgress, time);
    }

	public static void LoadScene(string name, FadeMode fadeMode, Color fadeColor, Action onComplete = null, Action<float> onProgress = null, float time = FadeTime)
    {
		OnSceneChanging?.Invoke(name);
		switch (fadeMode)
        {
            case FadeMode.None:
                {
					LoadScene(name, onComplete, onProgress);
                    break;
                }
            case FadeMode.FadeOut:
                {
                    FadeOut(fadeColor, () => {
						LoadScene(name, onComplete, onProgress);
					}, time);
                    break;
                }
            case FadeMode.FadeIn:
                {
                    LoadScene(name, () => {
							FadeIn(fadeColor, null, time);
						onComplete?.Invoke();
					}, onProgress);
                    break;
                }
            case FadeMode.FadeOutIn:
                {
                    FadeOut(fadeColor, () => {
						LoadScene(name, FadeMode.FadeIn, fadeColor, onComplete, onProgress, time);
					});
                    break;
                }
        }
    }

    public static void LoadScene(string name, Action onComplete = null, Action<float> onProgress = null)
    {
		var task = LoadSceneProgress(SceneManager.LoadSceneAsync(name), onComplete, onProgress);
	}

    public static void LoadSceneAdditive(string name, Action onComplete = null, Action<float> onProgress = null)
    {
		var task = LoadSceneProgress(SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive), onComplete, onProgress);
	}

    protected static async Task LoadSceneProgress(AsyncOperation load, Action onComplete = null, Action<float> onProgress = null)
    {
		while (!load.isDone)
        {
			onProgress?.Invoke(load.progress);
			await Observable.NextFrame();
        }
		onComplete?.Invoke();
	}

    public static void FadeIn(Action onComplete = null, float time = FadeTime)
    {
        FadeIn(FadeColor, onComplete, time);
    }

    public static void FadeOut(Action onComplete = null, float time = FadeTime)
    {
        FadeOut(FadeColor, onComplete, time);
    }

    public static void FadeIn(Color color, Action onComplete = null, float time = FadeTime)
    {
		OnFadingIn?.Invoke();
		Fade(color, 0, 1, () => {
			onComplete?.Invoke();
			OnFadedIn?.Invoke();
		}, time);
    }

    public static void FadeOut(Color color, Action onComplete = null, float time = FadeTime)
    {
		OnFadingOut?.Invoke();
		Fade(color, 1, 0, () => {
			onComplete?.Invoke();
			OnFadedOut?.Invoke();
		}, time);
    }

    public static void Fade(float to, Action onComplete = null, float time = FadeTime)
    {
        Fade(FadeImage.color, to, onComplete, time);
    }

    public static void Fade(Color color, float to, Action onComplete = null, float time = FadeTime)
    {
        Fade(color, FadeImage.color.a, to, onComplete, time);
    }

    public static void Fade(float from, float to, Action onComplete = null, float time = FadeTime)
    {
        Fade(FadeColor, from, to, onComplete, time);
    }

    public static void Fade(Color color, float from, float to, Action onComplete = null, float time = FadeTime)
    {
		OnFading?.Invoke(to, from);

		FadeImage.enabled = true;
		FadeImage.color = color.SetAlpha(1 - from);
        FadeImage.DOKill();
        FadeImage.DOFade(1 - to, time).OnComplete(() => {
			if (to >= 1)
				FadeImage.enabled = false;
			onComplete?.Invoke();
			OnFaded?.Invoke(to, from);
		});
    }

	protected static Image GetFadeImage()
    {
        Canvas canvas = Instance.gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = FadeOrder;
		Image image = Instance.gameObject.AddComponent<Image>();
        return image;
    }
	
    public static string ActiveScene
    {
        get
        {
            return SceneManager.GetActiveScene().name;
        }
    }

	public static bool IsMainScene
	{
		get
		{
			return ActiveScene == MainScene;
		}
	}

	public static bool IsSplashScene
	{
		get
		{
			return ActiveScene == SplashScene;
		}
	}

    public static Image FadeImage
    {
        get
        {
            if (fadeImage == null)
                fadeImage = GetFadeImage();
            return fadeImage;
        }
    }
}