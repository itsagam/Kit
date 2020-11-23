using Kit;
using UnityEngine;

namespace Demos.Pooling
{
	public class Game: MonoBehaviour
	{
		public AudioClip Music;

		protected void Start()
		{
			AudioManager.PlayMusic(Music);
		}
	}
}