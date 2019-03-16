﻿using Sirenix.OdinInspector;
using UniRx.Async;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Engine.UI.Buttons
{
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
		public UnityEvent LoadProgressed;

		[FoldoutGroup("Events")]
		public UnityEvent LoadCompleted;

		[FoldoutGroup("Events")]
		public UnityEvent Completed;

		public void OnPointerClick (PointerEventData eventData)
		{
			enabled = false;
			string scene = Reload ? SceneDirector.ActiveScene.path : Scene;
			SceneDirector.LoadScene(scene, FadeMode, FadeColor, FadeTime, false,
									progress => LoadProgressed.Invoke(), LoadCompleted.Invoke,
									Completed.Invoke)
			             .Forget();
		}
	}
}