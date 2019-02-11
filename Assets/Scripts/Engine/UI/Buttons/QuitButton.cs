using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

public class QuitButton : MonoBehaviour, IPointerClickHandler
{
	[ToggleGroup("Fade")]
	public bool Fade = true;

	[ToggleGroup("Fade")]
	public Color FadeColor = Color.black;

	[ToggleGroup("Fade")]
	public float FadeTime = 1.0f;

	public UnityEvent OnComplete;

	public void OnPointerClick (PointerEventData eventData)
	{
		if (Fade)
			SceneManager.FadeOut().SetColor(FadeColor).SetTime(FadeTime).OnComplete(Quit);
		else
			Quit();
	}

	public void Quit()
	{
		OnComplete.Invoke();
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif
		Application.Quit();
	}
}