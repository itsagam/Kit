using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Object = UnityEngine.Object;

public static class AudioManager
{
	private static GameObject audioGameObject;
	private static Transform audioTransform;
	private static Dictionary<string, AudioSource> groupSources = new Dictionary<string, AudioSource>();

	public const string BackgroundGroup = "Background";
	public const string SoundEffectGroup = "SoundEffect";
	public const string UIGroup = "UI";

	public static AudioFader BackgroundManager { get; private set; }

	static AudioManager()
	{
		Initialize();
	}

	#region Group management
	public static void Initialize()
	{
		if (audioGameObject != null)
			return;

		audioGameObject = new GameObject("Audio");
		audioTransform = audioGameObject.transform;

		AudioSource bgSource = CreateGroup(BackgroundGroup);
		BackgroundManager = bgSource.gameObject.AddComponent<AudioFader>();

		CreateGroup(SoundEffectGroup);
		CreateGroup(UIGroup);

		Object.DontDestroyOnLoad(audioGameObject);
	}

	public static AudioSource CreateGroup(string name)
	{
		GameObject gameObject = new GameObject(name);
		AudioSource source = gameObject.AddComponent<AudioSource>();
		source.transform.parent = audioTransform;
		groupSources.Add(name, source);
		return source;
	}

	public static AudioSource GetGroupSource(string name)
	{
		return groupSources.TryGetValue(name, out AudioSource source) ? source : null;
	}

	public static AudioSource GetOrCreateGroup(string name)
	{
		return groupSources.TryGetValue(name, out AudioSource source) ? source : CreateGroup(name);
	}

	public static bool RemoveGroup(string name)
	{
		var source = GetGroupSource(name);
		if (source == null)
			return false;

		groupSources.Remove(name);
		source.Destroy();
		return true;
	}

	public static IEnumerable<AudioSource> GetAllGroupSources()
	{
		return groupSources.Values;
	}
	#endregion

	#region Group playback
	public static void Play(string group, AudioClip clip)
	{
		if (clip != null)
			GetOrCreateGroup(group).PlayOneShot(clip);
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
		Play(SoundEffectGroup, clip);
	}

	public static void PlaySoundEffect(AudioClip[] clips)
	{
		if (clips == null || clips.Length <= 0)
			return;
		
		int randomIndex = UnityEngine.Random.Range(0, clips.Length);
		PlaySoundEffect(clips[randomIndex]);
	}

	public static void PlayUIEffect(AudioClip clip)
	{
		Play(UIGroup, clip);
	}
	#endregion

	#region AudioSource (Pooled) playback
	public static AudioSource Play(AudioSource prefab)
	{
		if (prefab == null)
			return null;

		var source = Pooler.Instantiate(prefab);
		QueueForDestroy(source);
		return source;
	}

	public static AudioSource Play(AudioSource prefab, Transform parent, bool worldSpace = false)
	{
		if (prefab == null)
			return null;

		var source = Pooler.Instantiate(prefab, parent, worldSpace);
		QueueForDestroy(source);
		return source;
	}

	public static AudioSource Play(AudioSource prefab, Vector3 position)
	{
		if (prefab == null)
			return null;

		var source = Pooler.Instantiate(prefab, position);
		QueueForDestroy(source);
		return source;
	}

	public static AudioSource Play(AudioSource prefab, Transform parent, Vector3 position)
	{
		if (prefab == null)
			return null;

		var source = Pooler.Instantiate(prefab);
		var transform = source.transform;
		transform.parent = parent;
		transform.localPosition = position;
		QueueForDestroy(source);
		return source;
	}

	private static void QueueForDestroy(AudioSource source)
	{
		if (!source.loop)
		{
			Observable.Timer(TimeSpan.FromSeconds(source.clip.length)).Subscribe(l =>
			{
				Pooler.Destroy(source);
			});
		}
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

		var source = PlayDedicated(clip, loop, is3D);
		source.transform.position = position;
		return source;
	}

	public static AudioSource Play(AudioClip clip, Transform parent, bool loop = false, bool is3D = true)
	{
		if (clip == null)
			return null;

		var source = PlayDedicated(clip, loop, is3D);
		source.transform.parent = parent;
		return source;
	}

	public static AudioSource Play(AudioClip clip, Transform parent, Vector3 position, bool loop = false, bool is3D = true)
	{
		if (clip == null)
			return null;

		var source = PlayDedicated(clip, loop, is3D);
		var transform = source.transform;
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
		{
			Observable.Timer(TimeSpan.FromSeconds(clip.length))
			.Subscribe(l =>
			{
				if (gameObject != null)
					gameObject.Destroy();
			});
		}

		return source;
	}
	#endregion

	#region Public fields
	public static AudioSource BackgroundSource => GetGroupSource(BackgroundGroup);
	public static AudioSource SoundEffectsSource => GetGroupSource(SoundEffectGroup);
	public static AudioSource UISource => GetGroupSource(UIGroup);
	public static float BackgroundMusicFadeSpeed
	{
		get => BackgroundManager.Speed;
		set => BackgroundManager.Speed = value;
	}
	public static bool IsBackgroundMusicPlaying => BackgroundManager.IsPlaying;
	#endregion
}