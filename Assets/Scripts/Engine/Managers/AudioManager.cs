using System;
using System.Collections.Generic;
using Engine.Pooling;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Engine
{
	public static class AudioManager
	{
		#region Fields

		public const string SoundGroup = "Sounds";
		public const string MusicGroup = "Music";
		public const string UIGroup = "UI";

		public static AudioFader BackgroundManager { get; private set; }

		private static GameObject audioGameObject;
		private static Transform audioTransform;
		private static Dictionary<string, AudioSource> groupSources = new Dictionary<string, AudioSource>();

		#endregion

		#region Initialization

		static AudioManager()
		{
			Initialize();
		}

		public static void Initialize()
		{
			if (audioGameObject != null)
				return;

			audioGameObject = new GameObject("Audio");
			audioTransform = audioGameObject.transform;

			AudioSource bgSource = CreateGroup(MusicGroup, true);
			BackgroundManager = bgSource.gameObject.AddComponent<AudioFader>();

			CreateGroup(SoundGroup, true);
			CreateGroup(UIGroup,    true);

			Object.DontDestroyOnLoad(audioGameObject);
		}

		#endregion

		#region Group management

		public static AudioSource CreateGroup(string name, bool loadVolume = false)
		{
			GameObject gameObject = new GameObject(name);
			AudioSource source = gameObject.AddComponent<AudioSource>();
			if (loadVolume)
				source.volume = LoadGroupVolume(name);
			source.transform.parent = audioTransform;
			groupSources.Add(name, source);
			return source;
		}

		public static AudioSource GetGroup(string name)
		{
			return groupSources.GetOrDefault(name);
		}

		public static AudioSource GetOrCreateGroup(string name)
		{
			return groupSources.TryGetValue(name, out AudioSource source) ? source : CreateGroup(name);
		}

		public static bool RemoveGroup(string name)
		{
			AudioSource source = GetGroup(name);
			if (source == null)
				return false;

			groupSources.Remove(name);
			source.gameObject.Destroy();
			return true;
		}

		public static IEnumerable<AudioSource> GetAllGroups()
		{
			return groupSources.Values;
		}

		public static float LoadGroupVolume(string name)
		{
			return PreferenceManager.Get("Audio", name, "Volume", 1.0f);
		}

		public static void SaveGroupVolume(string name, float volume)
		{
			PreferenceManager.Set("Audio", name, "Volume", volume);
		}

		#endregion

		#region Group playback

		public static void Play(string group, AudioClip clip)
		{
			if (clip != null)
				GetOrCreateGroup(group).PlayOneShot(clip);
		}

		public static void PlayMusic(AudioClip clip)
		{
			BackgroundManager.Play(clip);
		}

		public static void PlayMusic()
		{
			BackgroundManager.Play();
		}

		public static void PauseMusic()
		{
			BackgroundManager.Pause();
		}

		public static void StopMusic()
		{
			BackgroundManager.Stop();
		}

		public static void PlaySound(AudioClip clip)
		{
			Play(SoundGroup, clip);
		}

		public static void PlaySound(AudioClip[] clips)
		{
			if (clips == null || clips.Length <= 0)
				return;

			int randomIndex = Random.Range(0, clips.Length);
			PlaySound(clips[randomIndex]);
		}

		public static void PlayUI(AudioClip clip)
		{
			Play(UIGroup, clip);
		}

		#endregion

		#region AudioSource (Pooled) playback

		public static AudioSource Play(AudioSource prefab)
		{
			if (prefab == null)
				return null;

			AudioSource source = Pooler.Instantiate(prefab);
			QueueForDestroy(source);
			return source;
		}

		public static AudioSource Play(AudioSource prefab, Transform parent, bool worldSpace = false)
		{
			if (prefab == null)
				return null;

			AudioSource source = Pooler.Instantiate(prefab, parent, worldSpace);
			QueueForDestroy(source);
			return source;
		}

		public static AudioSource Play(AudioSource prefab, Vector3 position)
		{
			if (prefab == null)
				return null;

			AudioSource source = Pooler.Instantiate(prefab, position);
			QueueForDestroy(source);
			return source;
		}

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

		public static AudioSource Play(AudioClip clip, bool loop = false, bool is3D = false)
		{
			return clip == null ? null : PlayDedicated(clip, loop, is3D);
		}

		public static AudioSource Play(AudioClip clip, Vector3 position, bool loop = false, bool is3D = true)
		{
			if (clip == null)
				return null;

			AudioSource source = PlayDedicated(clip, loop, is3D);
			source.transform.position = position;
			return source;
		}

		public static AudioSource Play(AudioClip clip, Transform parent, bool loop = false, bool is3D = true)
		{
			if (clip == null)
				return null;

			AudioSource source = PlayDedicated(clip, loop, is3D);
			source.transform.parent = parent;
			return source;
		}

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

		public static AudioSource MusicSource => GetGroup(MusicGroup);
		public static AudioSource SoundSource => GetGroup(SoundGroup);
		public static AudioSource UISource => GetGroup(UIGroup);

		public static float MusicFadeSpeed
		{
			get => BackgroundManager.Speed;
			set => BackgroundManager.Speed = value;
		}

		public static bool IsMusicPlaying => BackgroundManager.IsPlaying;

		#endregion
	}
}