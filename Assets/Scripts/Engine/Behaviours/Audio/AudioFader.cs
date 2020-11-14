using System;
using DG.Tweening;
using UnityEngine;

namespace Engine.Behaviours
{
    /// <summary>
    /// Attach it to a <see cref="GameObject"/> with an <see cref="AudioSource" /> and this component will allow you to play, pause,
    /// stop, and change volume or clip with the volume fading in/out.
    /// </summary>
    /// <remarks>
    /// Just attaching it to an <see cref="AudioSource" /> with <see cref="AudioSource.playOnAwake" /> <see langword="true" /> will
    /// fade the audio in on Awake.
    /// </remarks>
    /// <seealso cref="SceneDirector"/>
    [RequireComponent(typeof(AudioSource))]
	public class AudioFader: MonoBehaviour
	{
        /// <summary>
        /// <para><see cref="AudioSource" /> to fade.</para>
		/// <para>Will use the one on the same <see cref="GameObject"/> if not specified.</para>
        /// </summary>
		[Tooltip("AudioSource to fade. Will use the one on the same GameObject by default.")]
		public AudioSource Audio;

		/// <summary>
		/// How fast to fade the audio.
		/// </summary>
		[Tooltip("How fast to fade the audio.")]
		public float Speed = 0.5f;

		/// <summary>
		/// Should the audio automatically fade in/out when the scene changes with <see cref="SceneDirector" />
		/// </summary>
		/// <returns><see langword="true"/> by default.</returns>
		[Tooltip("Should the audio automatically fade in/out when the scene changes with SceneDirector?")]
		public bool FadeOnSceneChange = true;

		/// <summary>
		/// Should the audio automatically fade in/out with the scene when you use <see cref="SceneDirector" />?
		/// </summary>
		/// <returns><see langword="false"/> by default.</returns>
		[Tooltip("Should the audio automatically fade in/out with the scene when you use SceneDirector?")]
		public bool FadeWithScreen = false;

		/// <summary>Returns whether the audio is fading (in or out).</summary>
		public bool IsBusy { get; protected set; }

		protected float lastVolume;
		protected bool lastPlaying;

		protected void Awake()
		{
			if (Audio == null)
				Audio = GetComponent<AudioSource>();

			if (Audio != null)
			{
				lastVolume = Audio.volume;
				if (Audio.clip != null && Audio.playOnAwake)
					ChangeFromTo(0, lastVolume);
			}
		}

		protected void OnEnable()
		{
			SceneDirector.SceneChanging += OnSceneChanging;
			SceneDirector.FadingOut += OnFadingOut;
			SceneDirector.FadingIn += OnFadingIn;
		}

		protected void OnDisable()
		{
			SceneDirector.SceneChanging -= OnSceneChanging;
			SceneDirector.FadingOut -= OnFadingOut;
			SceneDirector.FadingIn -= OnFadingIn;
		}

		protected void OnSceneChanging(string scene)
		{
			if (FadeOnSceneChange)
				Stop();
		}

		protected void OnFadingOut()
		{
			bool before = Audio.isPlaying;
			if (FadeWithScreen)
				Pause();
			lastPlaying = before;
		}

		protected void OnFadingIn()
		{
			if (FadeWithScreen && lastPlaying)
				Play();
		}

		/// <summary>
		/// Play an audio.
		/// </summary>
		/// <remarks>Fades-out the audio before fading-in if an audio is already playing.</remarks>
		public void Play(AudioClip clip)
		{
			if (clip == null)
				return;

			if (Audio.isPlaying)
			{
				if (IsBusy)
					Audio.DOKill();
				else
					lastVolume = Audio.volume;

				ChangeTo(0,
						 () =>
						 {
							 Audio.clip = clip;
							 Audio.Play();
							 ChangeTo(lastVolume);
						 });
			}
			else
			{
				Audio.clip = clip;
				Play();
			}
		}

		/// <summary>
		/// Play and fade-in the audio.
		/// </summary>
		public void Play()
		{
			lastPlaying = true;
			if (Audio.isPlaying)
			{
				if (IsBusy)
					Audio.DOKill();
				else
					lastVolume = Audio.volume;

				ChangeTo(lastVolume);
			}
			else
			{
				Audio.Play();
				ChangeFromTo(0, lastVolume);
			}
		}

		/// <summary>
		/// Pause and fade-out the audio.
		/// </summary>
		public void Pause()
		{
			lastPlaying = false;
			if (Audio.isPlaying)
			{
				if (IsBusy)
					Audio.DOKill();
				else
					lastVolume = Audio.volume;

				ChangeTo(0, Audio.Pause);
			}
			else
				Audio.Pause();
		}

		/// <summary>
		/// Stop and fade out the audio.
		/// </summary>
		public void Stop()
		{
			lastPlaying = false;
			if (Audio.isPlaying)
			{
				if (IsBusy)
					Audio.DOKill();
				else
					lastVolume = Audio.volume;

				ChangeTo(0, Audio.Stop);
			}
			else
				Audio.Stop();
		}

		protected Tweener ChangeFromTo(float from, float to, Action onComplete = null)
		{
			Audio.volume = from;
			return ChangeTo(to, onComplete);
		}

		protected Tweener ChangeFrom(float volume, Action onComplete = null)
		{
			return ChangeTo(volume, onComplete).From();
		}

		protected Tweener ChangeTo(float volume, Action onComplete = null)
		{
			IsBusy = true;
			return Audio.DOFade(volume, Speed)
						.SetSpeedBased()
						.SetEase(Ease.Linear)
						.OnComplete(() =>
									{
										IsBusy = false;
										onComplete?.Invoke();
									});
		}

		/// <summary>
		/// Returns the audio currently playing or allows to change it while fading.
		/// </summary>
		public AudioClip Clip
		{
			get => Audio.clip;
			set
			{
				if (Audio.isPlaying)
					Play(value);
				else
					Audio.clip = value;
			}
		}

		/// <summary>
		/// Returns the current volume or allows to set it while fading.
		/// </summary>
		public float Volume
		{
			get => Audio.volume;
			set
			{
				lastVolume = value;
				ChangeTo(value);
			}
		}

		/// <summary>
		/// Returns whether an audio is currently playing or allows to play or pause it.
		/// </summary>
		public bool IsPlaying
		{
			get => Audio.isPlaying;
			set
			{
				if (value)
					Play();
				else
					Pause();
			}
		}
	}
}