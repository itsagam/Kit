using UnityEngine;

namespace Engine.UI.Buttons
{
	/// <summary>
	/// Button that plays an audio on the UI audio group.
	/// </summary>
	public class AudioButton: ButtonBehaviour
	{
		public AudioClip Audio;

		protected override void OnClick()
		{
			AudioManager.PlayUI(Audio);
		}
	}
}