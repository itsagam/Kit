using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Async;

public class Test : MonoBehaviour
{
	public AudioClip Music;
	public AudioClip Clip;

	protected void Start()
	{
		AudioManager.Initialize();
	}

	public void Button()
	{
		//AudioManager.BackgroundMusicFadeSpeed = 0.15f;
		//AudioManager.PlayBackgroundMusic(Music);
		AudioManager.Play(Clip, Vector3.zero);

		/*
		Debugger.StartProfile("transform");
		for (int i = 0; i < 10000; i++)
		{
		}

		Debugger.EndProfile();

		
		Debugger.StartProfile("field");
		for (int i = 0; i < 10000; i++)
		{
		}
		Debugger.EndProfile();
		*/
	}
}
