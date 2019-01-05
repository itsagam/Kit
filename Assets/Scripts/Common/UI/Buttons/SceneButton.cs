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
	public UnityEvent OnProgress;
	public UnityEvent OnComplete;

	public void OnPointerClick (PointerEventData eventData)
	{
		if (Scene.IsNullOrEmpty())
			return;

		enabled = false;
		SceneHelper.LoadScene(Scene, FadeMode, FadeColor,
			() => {
				OnComplete.Invoke();
			},
			(float progress) => {
				OnProgress.Invoke();
			});
	}
}