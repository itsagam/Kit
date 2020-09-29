using Cysharp.Threading.Tasks;
using Engine.UI.Widgets;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Engine.UI.Buttons
{
	public class SceneButton : ButtonBehaviour
	{
		public bool Reload;

		[HideIf("Reload")]
		public SceneReference Scene;

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
		public UnityEvent LoadProgressed;

		[FoldoutGroup("Events")]
		public UnityEvent LoadCompleted;

		[FoldoutGroup("Events")]
		public UnityEvent Completed;

		protected override void OnClick()
		{
			string scene = Reload ? SceneDirector.ActiveScene.path : Scene;
			if (scene.IsNullOrEmpty())
				return;

			enabled = false;
			SceneDirector.LoadScene(scene, FadeMode, FadeColor, FadeTime, false,
									progress => LoadProgressed.Invoke(), LoadCompleted.Invoke,
									Completed.Invoke)
			             .Forget();
		}
	}
}