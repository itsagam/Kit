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
// TODO: Hook print function to Console
// TODO: Find a way to remove "CS."

// TODO: Make public methods static
// TODO: Provide multiple-line support

public class Console : MonoBehaviour
{
	public const bool Enabled = true;
	public const string Prefab = "Console/Console";
	public const int Length = 5000;
	public const string LogColor = "#DDDDDD";
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
		RegisterLogging();
		RegisterInput();
		InitializeLua();
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
	#endregion

	#region Console
	public void Show()
	{
		Animator.Play("Show");
		CommandInput.ActivateInputField();
		CommandInput.Select();
	}

	public void Hide()
	{
		Animator.Play("Hide");
		CommandInput.DeactivateInputField();
	}

	public void Toggle()
	{
		IsVisible = !IsVisible;
	}

	public bool IsVisible
	{
		get
		{
			var state = Animator.GetCurrentAnimatorStateInfo(0);
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

	public void Log(object obj)
	{
		Log(Debugger.ObjectOrEnumerableToString(obj, false));
	}

	public void Log(string line)
	{
		int newLength = log.Length + line.Length;
		if (newLength > Length)
		{
			int removeLength = newLength - Length;
			removeLength = log.IndexOf('\n', removeLength) + 1;
			log.Remove(0, removeLength);
		}
		log.AppendLine(line);
		LogText.text = log.ToString();
		Observable.NextFrame().Subscribe(t => ScrollToBottom());
	}

	public void ScrollToBottom()
	{
		LogScroll.verticalNormalizedPosition = 0;
	}

	public void ScrollToTop()
	{
		LogScroll.verticalNormalizedPosition = 1;
	}
	
	public void ClearLog()
	{
		log.Clear();
		LogText.text = "";
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

	public void ClearCommand()
	{
		CommandInput.ActivateInputField();
		CommandInput.Select();
		CommandInput.text = "";
	}
	#endregion

	#region Execution
	protected LuaEnv luaEnv;

	protected void InitializeLua()
	{
		luaEnv = new LuaEnv();
		string luaLibrary = ResourceManager.ReadText(ResourceFolder.StreamingAssets, "Lua/LuaLibrary.lua", false);
		luaEnv.DoString(luaLibrary);
	}

	protected void Update()
	{
		luaEnv.Tick();
	}

	public void Execute(string command)
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
			object[] results = luaEnv.DoString(commandActual);
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

	public void AddToHistory(string command)
	{
		history.Add(command);
		currentCommandIndex = history.Count;
	}

	public void SelectPreviousCommand()
	{
		SelectCommand(currentCommandIndex - 1);
	}

	public void SelectNextCommand()
	{
		SelectCommand(currentCommandIndex + 1);
	}

	public void SelectCommand(int index)
	{	
		if (index > history.Count - 1)
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

	public void ClearHistory()
	{
		history.Clear();
	}
	#endregion

	#region Destruction
	protected void OnDestroy()
	{
		luaEnv.Dispose();
		UnregisterLogging();
	}
	#endregion
}