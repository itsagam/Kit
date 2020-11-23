using System;
using System.Collections.Generic;
using Kit.Behaviours;
using Kit.Pooling;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kit
{
	/// <summary>Allows to play &amp; pool sounds and group them into <see cref="AudioSource" />s. Handles background music.</summary>
	public static class AudioManager
	{
		#region Fields

		/// <summary>The group name to use for general sounds.</summary>
		public const string SoundGroup = "Sounds";

		/// <summary>The group name to use for background music.</summary>
		public const string MusicGroup = "Music";

		/// <summary>The group name to use for UI sounds.</summary>
		public const string UIGroup = "UI";

		/// <summary>Handler for music since we're always fading it.</summary>
		public static AudioFader MusicManager { get; private set; }

		/// <summary>
		/// Default volume level to use for the background music.
		/// </summary>
		public const float DefaultMusicVolume = 0.75f;

		private static GameObject audioGameObject;
		private static Transform audioTransform;
		private static Dictionary<string, AudioSource> groupSources = new Dictionary<string, AudioSource>();

		#endregion

		#region Initialization

		static AudioManager()
		{
			Initialize();
		}

		/// <summary>Initializes the manager and creates audio groups for general sounds, UI and music.</summary>
		public static void Initialize()
		{
			if (audioGameObject != null)
				return;

			audioGameObject = new GameObject("Audio");
			audioTransform = audioGameObject.transform;

			AudioSource musicSource = CreateGroup(MusicGroup, LoadGroupVolume(MusicGroup, DefaultMusicVolume));
			musicSource.loop = true;
			MusicManager = musicSource.gameObject.AddComponent<AudioFader>();

			CreateGroup(SoundGroup, LoadGroupVolume(SoundGroup));
			CreateGroup(UIGroup,    LoadGroupVolume(UIGroup));

			Object.DontDestroyOnLoad(audioGameObject);
		}

		#endregion

		#region Group management

		/// <summary>Create a new <see cref="AudioSource" /> for a sound group.</summary>
		/// <param name="name">Name of the group.</param>
		/// <param name="volume">Volume level for the group sounds.</param>
		/// <returns>AudioSource created for the group.</returns>
		public static AudioSource CreateGroup(string name, float volume = 1.0f)
		{
			GameObject gameObject = new GameObject(name);
			AudioSource source = gameObject.AddComponent<AudioSource>();
			source.volume = volume;
			source.transform.parent = audioTransform;
			groupSources.Add(name, source);
			return source;
		}

		/// <summary>Get the <see cref="AudioSource" /> for a group.</summary>
		public static AudioSource GetGroup(string name)
		{
			return groupSources.GetOrDefault(name);
		}

		/// <summary>Get the <see cref="AudioSource" /> for a group and create a new one for it if it doesn't exist.</summary>
		public static AudioSource GetOrCreateGroup(string name)
		{
			return groupSources.TryGetValue(name, out AudioSource source) ? source : CreateGroup(name);
		}

		/// <summary>Destroy the <see cref="AudioSource" /> for a group.</summary>
		public static bool RemoveGroup(string name)
		{
			AudioSource source = GetGroup(name);
			if (source == null)
				return false;

			groupSources.Remove(name);
			source.gameObject.Destroy();
			return true;
		}

		/// <summary>Returns all <see cref="AudioSource" />s.</summary>
		public static IEnumerable<AudioSource> GetAllGroups()
		{
			return groupSources.Values;
		}

		/// <summary>Returns the volume saved in the settings for a group.</summary>
		/// <param name="name">Name of the group.</param>
		/// <param name="defaultVolume">Volume to return if it's not saved yet.</param>
		public static float LoadGroupVolume(string name, float defaultVolume = 1.0f)
		{
			return SettingsManager.Get("Audio", name, "Volume", defaultVolume);
		}

		/// <summary>Saves the current volume of a group in settings.</summary>
		/// <param name="name">Name of the group.</param>
		public static bool SaveGroupVolume(string name)
		{
			AudioSource source = GetGroup(name);
			if (source == null)
				return false;

			SaveGroupVolume(name, source.volume);
			return true;
		}

		/// <summary>Saves the volume of a group in settings.</summary>
		/// <param name="name">Name of the group.</param>
		/// <param name="volume">Volume level to save.</param>
		public static void SaveGroupVolume(string name, float volume)
		{
			SettingsManager.Set("Audio", name, "Volume", volume);
		}

		#endregion

		#region Group playback

		/// <summary>Play an audio with a group's <see cref="AudioSource" />. Create the group if it doesn't exist.</summary>
		public static void Play(string group, AudioClip clip)
		{
			if (clip != null)
				GetOrCreateGroup(group).PlayOneShot(clip);
		}

		/// <summary>Set and play background music.</summary>
		public static void PlayMusic(AudioClip clip)
		{
			MusicManager.Play(clip);
		}

		/// <summary>Play the background music.</summary>
		public static void PlayMusic()
		{
			MusicManager.Play();
		}

		/// <summary>Pause the background music.</summary>
		public static void PauseMusic()
		{
			MusicManager.Pause();
		}

		/// <summary>Stop the background music.</summary>
		public static void StopMusic()
		{
			MusicManager.Stop();
		}

		/// <summary>Play an audio with the general sounds group.</summary>
		public static void PlaySound(AudioClip clip)
		{
			Play(SoundGroup, clip);
		}

		/// <summary>Play a random audio from a list.</summary>
		public static void PlaySound(IReadOnlyList<AudioClip> clips)
		{
			if (clips == null || clips.Count <= 0)
				return;

			PlaySound(clips.GetRandom());
		}

		/// <summary>Play an audio with the UI sounds group.</summary>
		public static void PlayUI(AudioClip clip)
		{
			Play(UIGroup, clip);
		}

		#endregion

		#region AudioSource (Pooled) playback

		/// <summary>Spawn an <see cref="AudioSource" /> and pool it after the sound ends.</summary>
		/// <returns>The pool instance.</returns>
		public static AudioSource Play(AudioSource prefab)
		{
			if (prefab == null)
				return null;

			AudioSource source = Pooler.Instantiate(prefab);
			QueueForDestroy(source);
			return source;
		}

		/// <summary>Spawn an <see cref="AudioSource" /> and pool it after the sound ends.</summary>
		/// <returns>The pool instance.</returns>
		public static AudioSource Play(AudioSource prefab, Transform parent, bool worldSpace = false)
		{
			if (prefab == null)
				return null;

			AudioSource source = Pooler.Instantiate(prefab, parent, worldSpace);
			QueueForDestroy(source);
			return source;
		}

		/// <summary>Spawn an <see cref="AudioSource" /> and pool it after the sound ends.</summary>
		/// <returns>The pool instance.</returns>
		public static AudioSource Play(AudioSource prefab, Vector3 position)
		{
			if (prefab == null)
				return null;

			AudioSource source = Pooler.Instantiate(prefab, position);
			QueueForDestroy(source);
			return source;
		}

		/// <summary>Spawn an <see cref="AudioSource" /> and pool it after the sound ends.</summary>
		/// <returns>The pool instance.</returns>
		public static AudioSource Play(AudioSource prefab, Transform parent, Vector3 position)
		{
			if (prefab == null)
				return null;

			AudioSource source = Pooler.Instantiate(prefab);
			Transform transform = source.transform;
			transform.parent = parent;
			transform.localPosition = position;
			QueueForDestroy(source);
			return source;
		}

		private static void QueueForDestroy(AudioSource source)
		{
			if (!source.loop)
				Observable.Timer(TimeSpan.FromSeconds(source.clip.length)).Subscribe(l => Pooler.Destroy(source));
		}

		#endregion

		#region AudioClip (Unpooled) playback

		/// <summary>Play an audio with a dedicated <see cref="AudioSource" /> and destroy it after it ends (if it's not looping).</summary>
		/// <returns>The <see cref="AudioSource" /> instantiated.</returns>
		public static AudioSource Play(AudioClip clip, bool loop = false, bool is3D = false)
		{
			return clip == null ? null : PlayDedicated(clip, loop, is3D);
		}

		/// <summary>Play an audio with a dedicated <see cref="AudioSource" /> and destroy it after it ends (if it's not looping).</summary>
		/// <returns>The <see cref="AudioSource" /> instantiated.</returns>
		public static AudioSource Play(AudioClip clip, Vector3 position, bool loop = false, bool is3D = true)
		{
			if (clip == null)
				return null;

			AudioSource source = PlayDedicated(clip, loop, is3D);
			source.transform.position = position;
			return source;
		}

		/// <summary>Play an audio with a dedicated <see cref="AudioSource" /> and destroy it after it ends (if it's not looping).</summary>
		/// <returns>The <see cref="AudioSource" /> instantiated.</returns>
		public static AudioSource Play(AudioClip clip, Transform parent, bool loop = false, bool is3D = true)
		{
			if (clip == null)
				return null;

			AudioSource source = PlayDedicated(clip, loop, is3D);
			source.transform.parent = parent;
			return source;
		}

		/// <summary>Play an audio with a dedicated <see cref="AudioSource" /> and destroy it after it ends (if it's not looping).</summary>
		/// <returns>The <see cref="AudioSource" /> instantiated.</returns>
		public static AudioSource Play(AudioClip clip, Transform parent, Vector3 position, bool loop = false, bool is3D = true)
		{
			if (clip == null)
				return null;

			AudioSource source = PlayDedicated(clip, loop, is3D);
			Transform transform = source.transform;
			transform.parent = parent;
			transform.localPosition = position;
			return source;
		}

		private static AudioSource PlayDedicated(AudioClip clip, bool loop, bool is3D)
		{
			GameObject gameObject = new GameObject(clip.name);
			AudioSource source = gameObject.AddComponent<AudioSource>();
			source.clip = clip;
			source.loop = loop;

			if (is3D)
				source.spatialBlend = 1;

			source.Play();

			if (!loop)
				Observable.Timer(TimeSpan.FromSeconds(clip.length))
						  .Subscribe(l =>
									 {
										 if (gameObject != null)
											 gameObject.Destroy();
									 });

			return source;
		}

		#endregion

		#region Public properties

		/// <summary>The <see cref="AudioSource" /> for background music.</summary>
		public static AudioSource MusicSource => GetGroup(MusicGroup);

		/// <summary>The <see cref="AudioSource" /> for general sounds group.</summary>
		public static AudioSource SoundSource => GetGroup(SoundGroup);

		/// <summary>The <see cref="AudioSource" /> for UI sounds group.</summary>
		public static AudioSource UISource => GetGroup(UIGroup);

		/// <summary>Gets or sets fast to fade the background music.</summary>
		public static float MusicFadeSpeed
		{
			get => MusicManager.Speed;
			set => MusicManager.Speed = value;
		}

		/// <summary>Returns whether a background music is playing.</summary>
		public static bool IsMusicPlaying => MusicManager.IsPlaying;

		#endregion
	}
}