using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SceneButton : MonoBehaviour, IPointerClickHandler
{
	public string Scene;
	public FadeMode FadeMode = FadeMode.FadeOutIn;
	public Color FadeColor = Color.black;
	public float FadeTime = 1.0f;
	public UnityEvent OnProgress;
	public UnityEvent OnComplete;

	public void OnPointerClick (PointerEventData eventData)
	{
		if (Scene.IsNullOrEmpty())
			return;

		enabled = false;
		SceneManager.LoadScene(Scene).SetFadeMode(FadeMode).SetFadeColor(FadeColor).SetFadeTime(FadeTime)
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