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
	public UnityEvent OnProgress;
	public UnityEvent OnComplete;

	public void OnPointerClick (PointerEventData eventData)
	{
		enabled = false;
		SceneHelper.ReloadScene(FadeMode, FadeColor,
			() => {
				OnComplete.Invoke();
			},
			(float progress) => {
				OnProgress.Invoke();
			});
	}
}