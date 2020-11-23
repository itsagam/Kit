using Kit;
using UnityEngine;

namespace Demos.Pooling
{
	public class Shooter: MonoBehaviour
	{
		public AudioClip Music;

		protected void Start()
		{
			AudioManager.PlayMusic(Music);
		}
	}
}