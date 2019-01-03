using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;

public class Console : MonoBehaviour
{
	public const string PrefabPath = "Console/Console";
	public const int Length = 5000;
	public const double GestureTime = 250;
	public const float GestureDistance = 0.05f;
	protected static Console Instance = null;

	public Canvas Canvas;
	public Animator Animator;
	public ScrollRect Scroll;
	public Text Text;

	protected StringBuilder log = new StringBuilder(Length + 1);

	public static void Initialize()
	{
		Console prefab = Resources.Load<Console>(PrefabPath);
		Instance = Instantiate<Console>(prefab);
		Instance.name = prefab.name;
	}

	protected void Awake()
	{
		Application.logMessageReceived += OnLog;
		DontDestroyOnLoad(gameObject);
		IsVisible = false;
		Text.text = "";
		RegisterGesture();
	}

	protected void RegisterGesture()
	{
		var clickStream = Observable
			.EveryUpdate()
			.Where(l => Input.GetMouseButtonDown(0))
			.Select(l => Input.mousePosition / new Vector2(Screen.width, Screen.height));

		clickStream
			.Buffer(clickStream.Throttle(TimeSpan.FromMilliseconds(GestureTime)))
			.Where(b => b.Count >= 2)
			.Where(b => b.All(v => Math.Abs(v.x - b.Average(w => w.x)) + Math.Abs(v.y - b.Average(w => w.y)) < GestureDistance))
			.Subscribe(b => OnDoubleClick());
	}

	protected void OnDoubleClick()
	{
		IsVisible = !IsVisible;
	}
	
	protected void OnLog(string message, string stackTrace, LogType type)
	{
		LogLine(message);
		ScrollToBottom();
	}

	public void LogLine(string line)
	{
		int postLength = log.Length + line.Length;
		if (postLength > Length)
			log.Remove(0, postLength - Length);
		log.AppendLine(line);
		Text.text = log.ToString();
	}

	public void ScrollToBottom()
	{
		Scroll.verticalNormalizedPosition = 0;
	}

	public void ScrollToTop()
	{
		Scroll.verticalNormalizedPosition = 1;
	}

	public bool IsVisible
	{
		get
		{
			return Canvas.gameObject.activeSelf;
		}
		set
		{
			if (value)
			{
				Canvas.gameObject.SetActive(true);
				Scroll.transform.DOScaleY(1.0f, 0.4f).SetEase(Ease.InCubic);
			}
			else
			{
				Scroll.transform.DOScaleY(0.0f, 0.4f).SetEase(Ease.OutCubic).OnComplete(() =>
				{
					Canvas.gameObject.SetActive(false);
				});
			}
		}
	}
}