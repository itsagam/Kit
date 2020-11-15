using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Kit.UI.Buttons
{
	/// <summary>
	/// Button that loads a specific scene.
	/// </summary>
	public class SceneButton: ButtonBehaviour
	{
		/// <summary>
		/// Whether to just reload the current scene.
		/// </summary>
		[Tooltip("Whether to just reload the current scene.")]
		public bool Reload;

		/// <summary>
		/// Reference to the scene.
		/// </summary>
		[HideIf("Reload")]
		[Tooltip("Reference to the scene.")]
		public SceneReference Scene;

		/// <summary>
		/// The fading method to use.
		/// </summary>
		[FoldoutGroup("Fading")]
		[Tooltip("The fading method to use.")]
		public FadeMode FadeMode = FadeMode.FadeOutIn;

		/// <summary>
		/// The fading color to use.
		/// </summary>
		[FoldoutGroup("Fading")]
		[Tooltip("The fading color to use.")]
		[HideIf("FadeMode", FadeMode.None)]
		public Color FadeColor = Color.black;

		/// <summary>
		/// The fade duration.
		/// </summary>
		[FoldoutGroup("Fading")]
		[Tooltip("The fade duration.")]
		[HideIf("FadeMode", FadeMode.None)]
		[SuffixLabel("seconds", true)]
		public float FadeTime = 1.0f;

		/// <summary>
		/// Stuff to do when the loading progresses.
		/// </summary>
		[Tooltip("Stuff to do when the loading progresses.")]
		[FoldoutGroup("Events")]
		public UnityEvent LoadProgressed;

		/// <summary>
		/// Stuff to do when the loading completes.
		/// </summary>
		[Tooltip("Stuff to do when the loading completes.")]
		[FoldoutGroup("Events")]
		public UnityEvent LoadCompleted;

		/// <summary>
		/// Stuff to do when the loading and fading (if applicable) completes.
		/// </summary>
		[Tooltip("Stuff to do when the loading and fading (if applicable) completes.")]
		[FoldoutGroup("Events")]
		public UnityEvent Completed;

		protected override void OnClick()
		{
			string scene = Reload ? SceneDirector.ActiveScene.path : Scene;
			if (scene.IsNullOrEmpty())
				return;

			enabled = false;
			SceneDirector.LoadScene(scene,
									FadeMode,
									FadeColor,
									FadeTime,
									false,
									progress => LoadProgressed.Invoke(),
									LoadCompleted.Invoke,
									Completed.Invoke)
						 .Forget();
		}
	}
}