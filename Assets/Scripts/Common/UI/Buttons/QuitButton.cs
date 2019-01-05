using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class QuitButton : MonoBehaviour, IPointerClickHandler
{
	public Color FadeColor = Color.black;
	public UnityEvent OnComplete;

	public void OnPointerClick (PointerEventData eventData)
	{
		SceneHelper.FadeOut(FadeColor, () => {
			OnComplete.Invoke();
#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
#endif
			Application.Quit();
		});
	}
}