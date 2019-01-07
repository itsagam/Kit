using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;

// TODO: Set static variables
// TODO: Call static functions
// TODO: Set MonoBehavior variables
// TODO: Call MonoBehavior functions
// TODO: Command history
// TODO: Autocomplete objects
// TODO: Autocomplete functions/variables
// TODO: Autocomplete parameters

public class Console : MonoBehaviour
{
	public const bool Enabled = false;
	public const string PrefabPath = "Console/Console";
	public const int Length = 5000;
	public const double GestureTime = 250;
	public const float GestureDistance = 0.05f;
	public const float TransitionTime = 0.3f;
	public const string LogPrefix = "[Log] ";

	public Canvas Canvas;
	public ScrollRect LogScroll;
	public Text LogText;
	public InputField CommandInput;

	protected static Console Instance = null;
	protected StringBuilder log = new StringBuilder(Length + 1);
	
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void Initialize()
	{
		if (Enabled && Instance == null)
		{
			Console prefab = Resources.Load<Console>(PrefabPath);
			Instance = Instantiate(prefab);
			Instance.name = prefab.name;
		}
	}

	protected void Awake()
	{
		Application.logMessageReceived += OnLog;
		DontDestroyOnLoad(gameObject);
		Canvas.gameObject.SetActive(false);
		LogScroll.transform.localScale = new Vector3(1, 0, 1);
		RegisterGesture();

		LogText.text = "";
		CommandInput.text = "";
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
			.Subscribe(b =>
			{
				IsVisible = !IsVisible;
			});
	}
	
	protected void OnLog(string message, string stackTrace, LogType type)
	{
		LogLine(LogPrefix + message);
		ScrollToBottom();
	}

	public void LogLine(string line)
	{
		int postLength = log.Length + line.Length;
		if (postLength > Length)
			log.Remove(0, postLength - Length);
		log.AppendLine(line);
		LogText.text = log.ToString();
	}

	public void ScrollToBottom()
	{
		LogScroll.verticalNormalizedPosition = 0;
	}

	public void ScrollToTop()
	{
		LogScroll.verticalNormalizedPosition = 1;
	}

	public void Execute(string command)
	{

	}

	protected void OnDestroy()
	{
		Application.logMessageReceived -= OnLog;
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
				LogScroll.transform.DOScaleY(1.0f, TransitionTime).SetEase(Ease.InSine);
				CommandInput.ActivateInputField();
				CommandInput.Select();
				Observable.NextFrame().Subscribe(t => {
					CommandInput.MoveTextEnd(true);
				});
			}
			else
			{
				LogScroll.transform.DOScaleY(0.0f, TransitionTime).SetEase(Ease.OutSine).OnComplete(() =>
				{
					Canvas.gameObject.SetActive(false);
				});
			}
		}
	}
}