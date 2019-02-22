using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Combine PlayDedicated and CreateAudioSource
// TODO: Pool AudioSources

public static class AudioManager
{
	private static GameObject audioGameObject;
	private static Transform audioTransform;
	private static Dictionary<string, AudioSource> audioSources = new Dictionary<string, AudioSource>();

	public const string Background = "Background";
	public const string SoundEffect = "SoundEffect";
	public const string UI = "UI";

	public static AudioFader BackgroundManager { get; private set; }

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

		AudioSource bgSource = CreateAudioSource(Background, true);
		BackgroundManager = bgSource.gameObject.AddComponent<AudioFader>();

		CreateAudioSource(SoundEffect, true);
		CreateAudioSource(UI, true);

		GameObject.DontDestroyOnLoad(audioGameObject);
	}

	public static AudioSource CreateAudioSource(string name, bool is2D = false)
	{
		GameObject gameObject = new GameObject(name);
		AudioSource source = gameObject.AddComponent<AudioSource>();
		source.transform.parent = audioTransform;
		if (is2D)
			source.spatialBlend = 0;
		audioSources.Add(name, source);
		return source;
	}

	public static AudioSource GetAudioSource(string name)
	{
		if (audioSources.TryGetValue(name, out AudioSource source))
			return source;
		return null;
	}

	public static AudioSource GetOrCreateAudioSource(string name)
	{
		if (audioSources.TryGetValue(name, out AudioSource source))
			return source;
		return CreateAudioSource(name);
	}

	public static bool RemoveAudioSource(string name)
	{
		var source = GetAudioSource(name);
		if (source != null)
		{
			audioSources.Remove(name);
			source.Destroy();
			return true;
		}
		return false;
	}

	public static IEnumerable<AudioSource> GetAllAudioSources()
	{
		return audioSources.Values;
	}

	public static AudioSource Play(AudioClip clip, bool loop = false, bool is2D = true)
	{
		if (clip == null)
			return null;

		return PlayDedicated(clip, loop, is2D);
	}

	public static AudioSource Play(AudioClip clip, Vector3 position, bool loop = false, bool is2D = false)
	{
		if (clip == null)
			return null;

		var source = PlayDedicated(clip, loop, is2D);
		source.transform.position = position;
		return source;
	}

	public static AudioSource Play(AudioClip clip, Transform parent, bool loop = false, bool is2D = false)
	{
		if (clip == null)
			return null;

		var source = PlayDedicated(clip, loop, is2D);
		source.transform.parent = parent;
		return source;
	}

	public static AudioSource Play(AudioClip clip, Transform parent, Vector3 position, bool loop = false, bool is2D = false)
	{
		if (clip == null)
			return null;

		var source = PlayDedicated(clip, loop, is2D);
		var transform = source.transform;
		transform.parent = parent;
		transform.position = position;
		return source;
	}

	private static AudioSource PlayDedicated(AudioClip clip, bool loop, bool is2D)
	{
		GameObject gameObject = new GameObject(clip.name);
		AudioSource source = gameObject.AddComponent<AudioSource>();
		source.clip = clip;
		source.loop = loop;
		if (is2D)
			source.spatialBlend = 0;
		source.Play();
		return source;
	}

	public static void Play(string group, AudioClip clip)
	{
		if (clip != null)
			GetOrCreateAudioSource(group).PlayOneShot(clip);
	}

	public static void PlayBackgroundMusic(AudioClip clip)
	{
		BackgroundManager.Play(clip);
	}

	public static void PlayBackgroundMusic()
	{
		BackgroundManager.Play();
	}

	public static void PauseBackgroundMusic()
	{
		BackgroundManager.Pause();
	}

	public static void StopBackgroundMusic()
	{
		BackgroundManager.Stop();
	}

	public static void PlaySoundEffect(AudioClip clip)
	{
		Play(SoundEffect, clip);
	}

	public static void PlaySoundEffect(AudioClip[] clips)
	{
		if (clips != null && clips.Length > 0)
		{
			int randomIndex = UnityEngine.Random.Range(0, clips.Length);
			PlaySoundEffect(clips[randomIndex]);
		}
	}

	public static void PlayUIEffect(AudioClip clip)
	{
		Play(UI, clip);
	}

	public static AudioSource BackgroundSource
	{
		get
		{
			return GetAudioSource(Background);
		}
	}

	public static float BackgroundMusicFadeSpeed
	{
		get
		{
			return BackgroundManager.Speed;
		}
		set
		{
			BackgroundManager.Speed = value;
		}
	}

	public static bool IsBackgroundMusicPlaying
	{
		get
		{
			return BackgroundManager.IsPlaying;
		}
	}

	public static AudioSource SoundEffectsSource
	{
		get
		{
			return GetAudioSource(SoundEffect);
		}
	}

	public static AudioSource UISource
	{
		get
		{
			return GetAudioSource(UI);
		}
	}
}