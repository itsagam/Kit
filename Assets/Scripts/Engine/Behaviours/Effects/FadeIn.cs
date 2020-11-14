using UnityEngine;

namespace Engine.Behaviours
{
	/// <summary>
	/// Fades the audio in on Awake.
	/// </summary>
	/// <remarks>Drop it on a <see cref="GameObject"/> in a scene to fade it in whenever it loads.</remarks>
	public class FadeIn: MonoBehaviour
	{
		protected void Awake()
		{
			SceneDirector.FadeIn();
		}
	}
}