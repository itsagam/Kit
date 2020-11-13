using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Engine.UI.Buttons
{
	/// <summary>
	/// Button that quits the application.
	/// </summary>
	public class QuitButton: ButtonBehaviour
	{
		/// <summary>
		/// Should it fade the screen?
		/// </summary>
		[ToggleGroup("Fade")]
		[Tooltip("Should it fade the screen?")]
		public bool Fade = true;

		/// <summary>
		/// The color to use for fading.
		/// </summary>
		[ToggleGroup("Fade")]
		[Tooltip("The color to use for fading.")]
		public Color FadeColor = Color.black;

		/// <summary>
		/// How long to take to fade the screen?
		/// </summary>
		[ToggleGroup("Fade")]
		[SuffixLabel("seconds", true)]
		[Tooltip("How long to take to fade the screen?")]
		public float FadeTime = 1.0f;

		/// <summary>
		/// Stuff to do right at the end.
		/// </summary>
		[Tooltip("Stuff to do right at the end.")]
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