using UnityEngine;

namespace Engine.UI.Buttons
{
	public class AudioButton: ButtonBehaviour
	{
		public AudioClip Audio;

		protected override void OnClick()
		{
			AudioManager.PlayUI(Audio);
		}
	}
}