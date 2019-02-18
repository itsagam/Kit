using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UniRx;

public enum FadeMode
{
    None,
    FadeOut,
    FadeIn,
    FadeOutIn
}

public static class SceneManager
{
	public static event Action<string> OnSceneChanging;
	public static event Action<string> OnSceneChanged;
	public static event Action OnFadingIn;
	public static event Action OnFadedIn;
	public static event Action OnFadingOut;
	public static event Action OnFadedOut;
	public static event Action<float> OnFading;
	public static event Action<float> OnFaded;

	public static float DefaultFadeTime = 1.0f;
	public static Color DefaultFadeColor = Color.black;

	private static Image fadeImage;

	public class FadeBuilder
	{
		protected Tweener tween;
		protected event Action onComplete;

		public FadeBuilder(float to)
		{
			Execute(to);
		}

		public FadeBuilder SetFrom(float from)
		{
			tween.ChangeStartValue(FadeImage.color.SetAlpha(1 - from));
			return this;
		}

		public FadeBuilder SetTo(float to)
		{
			tween.ChangeEndValue(FadeImage.color.SetAlpha(1 - to));
			return this;
		}

		public FadeBuilder SetColor(Color color)
		{
			FadeImage.color = color.SetAlpha(FadeImage.color.a);
			return this;
		}

		public FadeBuilder SetTime(float time)
		{
			tween.timeScale = tween.Duration(false) / time;
			return this;
		}

		public FadeBuilder OnComplete(Action onComplete)
		{
			this.onComplete += onComplete;
			return this;
		}

		protected void Execute(float to)
		{
			OnFading?.Invoke(to);
			FadeImage.gameObject.SetActive(true);
			FadeImage.DOKill();
			FadeImage.color = DefaultFadeColor.SetAlpha(FadeImage.color.a);
			tween = FadeImage.DOFade(1 - to, DefaultFadeTime).OnComplete(() => {
				if (FadeImage.color.a <= 0)
					FadeImage.gameObject.SetActive(false);
				onComplete?.Invoke();
				OnFaded?.Invoke(to);
			});
		}
	}

	public class LoadBuilder
	{
		public string Name;
		public bool Additive;
		public FadeMode FadeMode = FadeMode.FadeOutIn;
		public Color FadeColor = DefaultFadeColor;
		public float FadeTime = DefaultFadeTime;

		protected event Action<float> onProgress;
		protected event Action onComplete;

		public LoadBuilder(string name)
		{
			Name = name;
			Observable.NextFrame().Subscribe(t => Execute());
		}

		public LoadBuilder SetAdditive(bool additive = true)
		{
			Additive = additive;
			return this;
		}

		public LoadBuilder SetFadeMode(FadeMode fadeMode)
		{
			FadeMode = fadeMode;
			return this;
		}

		public LoadBuilder SetFadeColor(Color fadeColor)
		{
			FadeColor = fadeColor;
			return this;
		}

		public LoadBuilder SetFadeTime(float fadeTime)
		{
			FadeTime = fadeTime;
			return this;
		}

		public LoadBuilder OnProgress(Action<float> onProgress)
		{
			this.onProgress += onProgress;
			return this;
		}

		public LoadBuilder OnComplete(Action onComplete)
		{
			this.onComplete += onComplete;
			return this;
		}

		protected void Execute()
		{
			if (!Additive)
				OnSceneChanging?.Invoke(Name);

			switch (FadeMode)
			{
				case FadeMode.None:
					{
						LoadSceneInternal(Name, Additive, onProgress, onComplete);
						break;
					}
				case FadeMode.FadeOut:
					{
						FadeOut().SetColor(FadeColor).SetTime(FadeTime).OnComplete(() =>
						{
							LoadSceneInternal(Name, Additive, onProgress, onComplete);
						});
						break;
					}
				case FadeMode.FadeIn:
					{
						LoadSceneInternal(Name, Additive, onProgress, () => {
							FadeIn().SetColor(FadeColor).SetTime(FadeTime);
							onComplete?.Invoke();
						});
						break;
					}
				case FadeMode.FadeOutIn:
					{
						FadeOut().SetColor(FadeColor).SetTime(FadeTime).OnComplete(() => {
							LoadScene(Name).SetFadeMode(FadeMode.FadeIn).SetFadeColor(FadeColor).SetFadeTime(FadeTime).OnProgress(onProgress).OnComplete(onComplete);
						});
						break;
					}
			}
		}
	}

	public static LoadBuilder LoadScene(string name)
	{
		return new LoadBuilder(name);
	}

	public static LoadBuilder ReloadScene()
	{
		return LoadScene(ActiveScene);
	}

	private static void LoadSceneInternal(string name, bool additive, Action<float> onProgress, Action onComplete)
	{
		AsyncOperation load = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(name, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
		MainThreadDispatcher.StartUpdateMicroCoroutine(LoadSceneProgress(load, onProgress, () => {
			onComplete?.Invoke();
			if (!additive)
				OnSceneChanged?.Invoke(name);
		}));
	}

	private static IEnumerator LoadSceneProgress(AsyncOperation load, Action<float> onProgress, Action onComplete)
    {
		while (!load.isDone)
        {
			onProgress?.Invoke(load.progress);
			yield return null;
        }
		onComplete?.Invoke();
	}

	public static FadeBuilder Fade(float to)
	{
		return new FadeBuilder(to);
	}

	public static FadeBuilder FadeIn()
	{
		OnFadingIn?.Invoke();
		return new FadeBuilder(1).SetFrom(0).OnComplete(() =>
		{
			OnFadedIn?.Invoke();
		});
	}

	public static FadeBuilder FadeOut()
	{
		OnFadingOut?.Invoke();
		return new FadeBuilder(0).SetFrom(1).OnComplete(() =>
		{
			OnFadedOut?.Invoke();
		});
	}

	private static Image GetFadeImage()
    {
		GameObject gameObject = new GameObject(typeof(SceneManager).Name);
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 99999;
		Image image = gameObject.AddComponent<Image>();
		GameObject.DontDestroyOnLoad(gameObject);
        return image;
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

	public static string ActiveScene
    {
        get
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }
    }

	public static bool IsScene(string name)
	{
		return ActiveScene == name;
	}
}