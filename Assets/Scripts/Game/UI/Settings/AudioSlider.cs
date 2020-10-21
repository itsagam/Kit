using Engine;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Settings
{
	[RequireComponent(typeof(Slider))]
	public class AudioSlider: MonoBehaviour
	{
		public string Group = "Sounds";

		protected Slider slider;
		protected AudioSource audioSource;

		protected void Awake()
		{
			audioSource = AudioManager.GetGroup(Group);

			slider = GetComponent<Slider>();
			slider.onValueChanged.AddListener(SetVolume);
			slider.value = audioSource.volume;
		}

		protected void SetVolume(float volume)
		{
			audioSource.volume = volume;
			AudioManager.SaveGroupVolume(Group, volume);
		}
	}
}