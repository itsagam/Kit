using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

public class SceneButton : MonoBehaviour, IPointerClickHandler
{
	[TitleGroup("Scene")]
	[HideIf("Reload")]
	public string Scene;
	public bool Reload;

	[TitleGroup("Fading")]
	public FadeMode FadeMode = FadeMode.FadeOutIn;

	[HideIf("FadeMode", FadeMode.None)]
	public Color FadeColor = Color.black;

	[HideIf("FadeMode", FadeMode.None)]
	public float FadeTime = 1.0f;

	[TitleGroup("Events")]
	public UnityEvent OnProgress;
	public UnityEvent OnComplete;

	public void OnPointerClick (PointerEventData eventData)
	{
		SceneManager.LoadBuilder builder = null;
		if (Reload)
			builder = SceneManager.ReloadScene();
		else
			builder = SceneManager.LoadScene(Scene);

		enabled = false;
		builder.SetFadeMode(FadeMode).SetFadeColor(FadeColor).SetFadeTime(FadeTime)
			.OnProgress((float progress) =>
			{
				OnProgress.Invoke();
			})
			.OnComplete(() =>
			{
				OnComplete.Invoke();
			});
	}
}