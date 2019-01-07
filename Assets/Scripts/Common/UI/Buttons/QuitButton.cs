using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class QuitButton : MonoBehaviour, IPointerClickHandler
{
	public bool Fade = true;
	public Color FadeColor = Color.black;
	public float FadeTime = 1.0f;
	public UnityEvent OnComplete;

	public void OnPointerClick (PointerEventData eventData)
	{
		if (Fade)
			SceneManager.FadeOut().SetColor(FadeColor).SetTime(FadeTime).OnComplete(Quit).Execute();
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