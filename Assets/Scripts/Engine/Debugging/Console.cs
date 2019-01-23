using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TouchScript.Gestures;
using UniRx;
using XLua;

// TODO: Provide a way to list all members
// TODO: Provide hotfix helper functions
// TODO: Provide a way to handle files
// TODO: Find a way to remove "CS."

// TODO: Provide multiple-line support

public class Console : MonoBehaviour
{
	public const bool Enabled = true;
	public const string Prefab = "Console/Console";
	public const int Length = 5000;
	public const string LogColor = "#00DDFF"; //#DDDDDDD
	public const string CommandPrefix = "> ";
	public const string NullString = "nil";

	public Animator Animator;
	public ScrollRect LogScroll;
	public Text LogText;
	public CustomInputField CommandInput;

	protected static Console instance = null;

	#region Initialization
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void Initialize()
	{
		if (Enabled && instance == null)
		{
			Console prefab = Resources.Load<Console>(Prefab);
			instance = Instantiate(prefab);
			instance.name = prefab.name;
		}
	}

	protected void Awake()
	{
		instance = this;

		RegisterLogging();
		RegisterInput();
		InitializeScripting();
		InitializeUI();
		InitializeHistory();
		DontDestroyOnLoad(gameObject);
	}

	protected void RegisterInput()
	{
		if (Application.isMobilePlatform)
		{
			var flick = GetComponent<FlickGesture>();
			flick.Flicked += (object o, EventArgs e) => {
				if (flick.ScreenFlickVector.y < 0 && !IsVisible)
					Show();
				else if (flick.ScreenFlickVector.y > 0 && IsVisible)
					Hide();
			};
		}
		else
		{
			var keyStream = Observable
				.EveryUpdate()
				.Where(l => Input.GetKeyDown(KeyCode.BackQuote))
				.Subscribe(l => Toggle())
				.AddTo(this);
		}

		/*
		// Toggle Console with double-tap (without TouchScript)
		const double GestureTime = 250;
		const float GestureDistance = 0.05f;
		const float TransitionTime = 0.3f;

		var clickStream = Observable
			.EveryUpdate()
			.Where(l => Input.GetMouseButtonDown(0))
			.Select(l => Input.mousePosition / new Vector2(Screen.width, Screen.height));

		clickStream
			.Buffer(clickStream.Throttle(TimeSpan.FromMilliseconds(GestureTime)))
			.Where(b => b.Count >= 2)
			.Where(b => b.All(v => Math.Abs(v.x - b.Average(w => w.x)) + Math.Abs(v.y - b.Average(w => w.y)) < GestureDistance))
			.Subscribe(b => Toggle())
			.AddTo(this);
		*/

		CommandInput.AddKeyHandler(KeyCode.Return, NewLine, EventModifiers.Shift);
		CommandInput.AddKeyHandler(KeyCode.Return, Submit);
		CommandInput.AddKeyHandler(KeyCode.UpArrow, SelectPreviousCommand);
		CommandInput.AddKeyHandler(KeyCode.DownArrow, SelectNextCommand);
	}
	#endregion

	#region Console
	protected void InitializeUI()
	{
		LogText.text = "";
		CommandInput.text = "";
		CommandInput.onValidateInput += OnValidateInput;
	}

	protected void NewLine()
	{
		//CommandInput.caretPosition;
	}

	protected char OnValidateInput(string text, int charIndex, char addedChar)
	{
		if (addedChar == '`')
			return '\0';
		//else if (addedChar == '\n')
		//	return '\0';
		else
			return addedChar;
	}

	public static void Show()
	{
		instance.Animator.Play("Show");
		instance.CommandInput.ActivateInputField();
		instance.CommandInput.Select();
	}

	public static void Hide()
	{
		instance.Animator.Play("Hide");
		instance.CommandInput.DeactivateInputField();
	}

	public static void Toggle()
	{
		IsVisible = !IsVisible;
	}

	public static bool IsVisible
	{
		get
		{
			var state = instance.Animator.GetCurrentAnimatorStateInfo(0);
			if (state.IsName("Show"))
				return state.normalizedTime > 1;
			else if (state.IsName("Hide"))
				return state.normalizedTime < 1;
			return false;
		}
		set
		{
			if (value)
				Show();
			else
				Hide();
		}
	}
	#endregion

	#region Log
	protected StringBuilder log = new StringBuilder(Length);

	public void RegisterLogging()
	{
		Application.logMessageReceived += OnLog;
	}

	public void UnregisterLogging()
	{
		Application.logMessageReceived -= OnLog;
	}

	protected void OnLog(string message, string stackTrace, LogType type)
	{
		Log($"<color={LogColor}>{message}</color>");
	}

	public static void Log(object obj)
	{
		Log(Debugger.ObjectOrEnumerableToString(obj, false));
	}

	public static void Log(string line)
	{
		var log = instance.log;

		int newLength = log.Length + line.Length;
		if (newLength > Length)
		{
			int removeLength = newLength - Length;
			removeLength = log.IndexOf('\n', removeLength) + 1;
			log.Remove(0, removeLength);
		}
		log.AppendLine(line);
		instance.LogText.text = log.ToString();
		Observable.NextFrame().Subscribe(t => ScrollToBottom());
	}

	public static void ScrollToBottom()
	{
		instance.LogScroll.verticalNormalizedPosition = 0;
	}

	public static void ScrollToTop()
	{
		instance.LogScroll.verticalNormalizedPosition = 1;
	}
	
	public static void ClearLog()
	{
		instance.log.Clear();
		instance.LogText.text = "";
	}
	#endregion

	#region Command
	protected void Submit()
	{
		string command = CommandInput.text;
		if (command != "")
		{
			Log(CommandPrefix + command);
			AddToHistory(command);
			Execute(command);
			ClearCommand();
		}
	}

	public static void ClearCommand()
	{
		instance.CommandInput.ActivateInputField();
		instance.CommandInput.Select();
		instance.CommandInput.text = "";
	}
	#endregion

	#region Execution
	protected LuaEnv scriptEnv;

	protected void InitializeScripting()
	{
		scriptEnv = new LuaEnv();
		scriptEnv.DoString("require 'Lua/General'");
		scriptEnv.DoString("require 'Lua/Console'");
	}

	protected void Update()
	{
		scriptEnv.Tick();
	}

	public static void Execute(string command)
	{
		try
		{
			// Try to execute as an expression first
			ExecuteLocal("return " + command);
		}
		catch (LuaException)
		{
			try
			{
				ExecuteLocal(command);
			}
			catch (LuaException ex)
			{
				Log(ex.Message);
			}
		}

		void ExecuteLocal(string commandActual)
		{
			object[] results = instance.scriptEnv.DoString(commandActual);
			results?.ForEach(r => Log(r != null ? r.ToString() : NullString));
		}
	}
	#endregion

	#region History
	protected List<string> history;
	protected int currentCommandIndex;

	protected void InitializeHistory()
	{
		history = new List<string>();
		currentCommandIndex = 0;
	}

	protected void AddToHistory(string command)
	{
		history.Add(command);
		currentCommandIndex = history.Count;
	}

	protected void SelectPreviousCommand()
	{
		SelectCommand(currentCommandIndex - 1);
	}

	protected void SelectNextCommand()
	{
		SelectCommand(currentCommandIndex + 1);
	}

	protected void SelectCommand(int index)
	{	
		if (index >= history.Count)
		{
			CommandInput.text = "";
			currentCommandIndex = history.Count;
		}
		else if (index < 0)
		{
		}
		else
		{
			CommandInput.text = history[index];
			currentCommandIndex = index;
		}
		CommandInput.MoveTextEnd(false);
	}

	public static void ClearHistory()
	{
		instance.history.Clear();
	}
	#endregion

	#region Destruction
	protected void OnDestroy()
	{
		scriptEnv.Dispose();
		UnregisterLogging();
	}
	#endregion
}