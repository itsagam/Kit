using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

public class SceneButton : MonoBehaviour, IPointerClickHandler
{
	[HideIf("Reload")]
	public string Scene;

	public bool Reload;

	[FoldoutGroup("Fading")]
	public FadeMode FadeMode = FadeMode.FadeOutIn;

	[FoldoutGroup("Fading")]
	[HideIf("FadeMode", FadeMode.None)]
	public Color FadeColor = Color.black;

	[FoldoutGroup("Fading")]
	[HideIf("FadeMode", FadeMode.None)]
	[SuffixLabel("seconds", true)]
	public float FadeTime = 1.0f;


	[FoldoutGroup("Events")]
	public UnityEvent OnProgress;

	[FoldoutGroup("Events")]
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