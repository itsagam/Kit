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

public class SceneManager
{
	public static event Action<string> OnSceneChanging;
	public static event Action OnFadingIn;
	public static event Action OnFadedIn;
	public static event Action OnFadingOut;
	public static event Action OnFadedOut;
	public static event Action<float, float> OnFading;
	public static event Action<float, float> OnFaded;

	protected static Image fadeImage;

	public class FadeBuilder
	{
		public float To;
		public float From;
		public Color Color = Color.black;
		public float Time = 1.0f;
		
		protected event Action onComplete;

		public FadeBuilder(float to)
		{
			To = to;
		}

		public FadeBuilder SetFrom(float from)
		{
			From = from;
			return this;
		}

		public FadeBuilder SetTo(float to)
		{
			To = to;
			return this;
		}

		public FadeBuilder SetColor(Color color)
		{
			Color = color;
			return this;
		}

		public FadeBuilder SetTime(float time)
		{
			Time = time;
			return this;
		}

		public FadeBuilder OnComplete(Action onComplete)
		{
			this.onComplete += onComplete;
			return this;
		}

		public void Execute()
		{
			OnFading?.Invoke(To, From);

			FadeImage.enabled = true;
			FadeImage.color = Color.SetAlpha(1 - From);
			FadeImage.DOKill();
			FadeImage.DOFade(1 - To, Time).OnComplete(() => {
				if (To >= 1)
					FadeImage.enabled = false;
				onComplete?.Invoke();
				OnFaded?.Invoke(To, From);
			});
		}
	}

	public class LoadBuilder
	{
		public string Name;
		public bool Additive = false;
		public FadeMode FadeMode = FadeMode.FadeOutIn;
		public Color FadeColor = Color.black;
		public float FadeTime = 1.0f;

		protected event Action<float> onProgress;
		protected event Action onComplete;

		public LoadBuilder(string name)
		{
			Name = name;
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

		public void Execute()
		{
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
						}).Execute();
						break;
					}
				case FadeMode.FadeIn:
					{
						LoadSceneInternal(Name, Additive, onProgress, () => {
							FadeIn().SetColor(FadeColor).SetTime(FadeTime).Execute();
							onComplete?.Invoke();
						});
						break;
					}
				case FadeMode.FadeOutIn:
					{
						FadeOut().SetColor(FadeColor).SetTime(FadeTime).OnComplete(() =>
						{
							LoadScene(Name).SetFadeMode(FadeMode.FadeIn).SetFadeColor(FadeColor).SetFadeTime(FadeTime).OnProgress(onProgress).OnComplete(onComplete).Execute();
						}).Execute();
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

	protected static void LoadSceneInternal(string name, bool additive, Action<float> onProgress, Action onComplete)
	{
		var task = LoadSceneProgress(UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(name, additive ? LoadSceneMode.Additive : LoadSceneMode.Single), onProgress, onComplete);
	}

	protected static async Task LoadSceneProgress(AsyncOperation load, Action<float> onProgress, Action onComplete)
    {
		while (!load.isDone)
        {
			onProgress?.Invoke(load.progress);
			await Observable.NextFrame();
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

	protected static Image GetFadeImage()
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