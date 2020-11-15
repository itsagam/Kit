using UnityEngine;

namespace Kit.UI.Buttons
{
	/// <summary>
	/// Button that plays an audio on the UI audio group.
	/// </summary>
	public class AudioButton: ButtonBehaviour
	{
		/// <summary>
		/// Audio to play when the button is clicked.
		/// </summary>
		[Tooltip("Audio to play when the button is clicked.")]
		public AudioClip Audio;

		protected override void OnClick()
		{
			AudioManager.PlayUI(Audio);
		}
	}
}