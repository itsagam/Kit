using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class AudioHelper : MonoBehaviour
{
    public AudioSource Audio;
    public float Speed = 0.75f;
    public bool FadeOnSceneChange = true;
    public bool FadeWithScreen = false;
    public bool IsBusy { get; protected set; }

    protected float lastVolume;
    protected bool lastPlaying;

    protected void Awake()
    {
        if (Audio == null)
            Audio = GetComponent<AudioSource>();
        lastVolume = Audio.volume;
        if (Audio.playOnAwake)
            ChangeFromTo(0, lastVolume);
    }

    protected void OnEnable()
    {
		SceneHelper.OnSceneChanging += OnSceneChanging;
		SceneHelper.OnFadingOut += OnFadingOut;
		SceneHelper.OnFadingIn += OnFadingIn;
    }

    protected void OnDisable()
    {
		SceneHelper.OnSceneChanging -= OnSceneChanging;
		SceneHelper.OnFadingOut -= OnFadingOut;
		SceneHelper.OnFadingIn -= OnFadingIn;
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

    public void Play()
    {
        lastPlaying = true;
		if (Audio.isPlaying)
		{
			if (IsBusy)
				Audio.DOKill(false);
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

    public void Pause()
    {
        lastPlaying = false;
        if (Audio.isPlaying)
        {
			if (IsBusy)
				Audio.DOKill(false);
			else
				lastVolume = Audio.volume;
			
            ChangeTo(0, () => {
                    Audio.Pause();		
                    });
        }
        else
            Audio.Pause();
    }

    public void Stop()
    {
        lastPlaying = false;
        if (Audio.isPlaying)
        {
			if (IsBusy)
				Audio.DOKill(false);
			else
				lastVolume = Audio.volume;
			
            ChangeTo(0, () => {
                    Audio.Stop();		
                    });
        }
        else
            Audio.Stop();
    }

    public void Change(AudioClip clip)
    {
		if (clip == null)
			return;
		
        if (Audio.isPlaying)
        {
			if (IsBusy)
				Audio.DOKill(false);
			else
				lastVolume = Audio.volume;
			
            ChangeTo(0, () => {
                    Audio.clip = clip;
                    Audio.Play();
                    ChangeTo(lastVolume);
                    });
        }
        else
            Audio.clip = clip;
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
		return Audio.DOFade(volume, Speed).SetSpeedBased().SetEase(Ease.Linear).OnComplete(() => {
                IsBusy = false;
			onComplete?.Invoke();
		});
    }

    public AudioClip Clip
    {
        get
        {
            return Audio.clip;
        }
        set
        {
            Change(value);
        }
    }

    public float Volume
    {
        get
        {
            return Audio.volume;
        }
        set
        {
            lastVolume = value;
            ChangeTo(value);
        }
    }

    public bool IsPlaying
    {
        get
        {
            return Audio.isPlaying;
        }
        set
        {
            if (value)
                Play();
            else
                Stop();
        }
    }

    public static AudioHelper Main
    {
        get
        {
            return Camera.main.GetComponent<AudioHelper>();
        }
    }
}