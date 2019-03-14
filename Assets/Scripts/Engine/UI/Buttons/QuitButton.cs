using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Engine.UI.Buttons
{
	public class QuitButton : MonoBehaviour, IPointerClickHandler
	{
		[ToggleGroup("Fade")]
		public bool Fade = true;

		[ToggleGroup("Fade")]
		public Color FadeColor = Color.black;

		[ToggleGroup("Fade")]
		[SuffixLabel("seconds", true)]
		public float FadeTime = 1.0f;

		public UnityEvent OnComplete;

		public void OnPointerClick (PointerEventData eventData)
		{
			if (Fade)
				SceneDirector.FadeOut(FadeColor, FadeTime, Quit);
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
}