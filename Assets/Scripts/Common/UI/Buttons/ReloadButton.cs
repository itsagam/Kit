using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ReloadButton : MonoBehaviour, IPointerClickHandler
{
	public FadeMode FadeMode = FadeMode.FadeOutIn;
	public Color FadeColor = Color.black;
	public float FadeTime = 1.0f;
	public UnityEvent OnProgress;
	public UnityEvent OnComplete;

	public void OnPointerClick (PointerEventData eventData)
	{
		enabled = false;
		SceneManager.ReloadScene().SetFadeMode(FadeMode).SetFadeColor(FadeColor).SetFadeTime(FadeTime)
			.OnProgress((float progress) => {
				OnProgress.Invoke();
			})
			.OnComplete(() => {
				OnComplete.Invoke();
			});
	}
}