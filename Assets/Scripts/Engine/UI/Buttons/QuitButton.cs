using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Engine.UI.Buttons
{
	public class QuitButton: ButtonBehaviour
	{
		[ToggleGroup("Fade")]
		public bool Fade = true;

		[ToggleGroup("Fade")]
		public Color FadeColor = Color.black;

		[ToggleGroup("Fade")]
		[SuffixLabel("seconds", true)]
		public float FadeTime = 1.0f;

		public UnityEvent Completed;

		protected override void OnClick()
		{
			if (Fade)
				SceneDirector.FadeOut(FadeColor, FadeTime, Quit);
			else
				Quit();
		}

		public void Quit()
		{
			Application.Quit();
			Completed.Invoke();
#if UNITY_EDITOR
			EditorApplication.isPlaying = false;
#endif
		}
	}
}