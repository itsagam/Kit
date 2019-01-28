using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TouchScript.Layers;
using TouchScript.Gestures;
using UniRx;
using XLua;

// UNDONE: Find a way to set default values in MonoBehaviours (LoadMerged with JSON/Lua...)
// TODO: Provide hotfix functions

public class Console : MonoBehaviour
{
	public Animator Animator;
	public ScrollRect LogScroll;
	public Text LogText;
	public InputFieldEx CommandInput;
	public ContentSizeFitter CommandInputFitter;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
	public const string Prefab = "Console/Console";
	public const int Length = 5000;
	public const string LogColor = "#7EF9FF";
	public const string CommandPrefix = "> ";
	public const string NullString = "nil";
	public static int Depth = 2;

	protected static Console instance = null;

	#region Initialization
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	public static void Initialize()
	{
		if (instance == null)
		{
			Console prefab = Resources.Load<Console>(Prefab);
			instance = Instantiate(prefab);
			instance.name = prefab.name;
		}

		if (EventSystem.current == null)
			new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
	}

	protected void Awake()
	{
		instance = this;
		
		RegisterLogging();
		RegisterInput();
		InitializeUI();
		InitializeScripting();
		DontDestroyOnLoad(gameObject);
	}

	protected void RegisterInput()
	{
		if (!(Application.isMobilePlatform || Application.isConsolePlatform))
		{
			var keyStream = Observable
				.EveryUpdate()
				.Where(l => Input.GetKeyDown(KeyCode.BackQuote))
				.Subscribe(l => Toggle())
				.AddTo(this);
		}
		else
		{
			var layer = gameObject.AddComponent<FullscreenLayer>();
			layer.Type = FullscreenLayer.LayerType.Global;

			var flick = gameObject.AddComponent<FlickGesture>();
			flick.Direction = FlickGesture.GestureDirection.Vertical;
			flick.Flicked += (object o, EventArgs e) => {
				if (flick.ScreenFlickVector.y < 0 && !IsVisible)
					Show();
				else if (flick.ScreenFlickVector.y > 0 && IsVisible)
					Hide();
			};
		}

		EventModifiers disregard = EventModifiers.FunctionKey | EventModifiers.Numeric | EventModifiers.CapsLock;
		var input = CommandInput;
		input.AddKeyHandler(KeyCode.BackQuote,	() =>	{},											EventModifiers.None,	disregard);
		input.AddKeyHandler(KeyCode.Return,				Submit,										EventModifiers.None,	disregard);
		input.AddKeyHandler(KeyCode.Return,		() =>	input.SendKeyEvent(KeyCode.Return,'\n'),	EventModifiers.Shift,	disregard);
		input.AddKeyHandler(KeyCode.UpArrow,			SelectPreviousCommand,						EventModifiers.None,	disregard);
		input.AddKeyHandler(KeyCode.DownArrow,			SelectNextCommand,							EventModifiers.None,	disregard);
		input.AddKeyHandler(KeyCode.UpArrow,	() =>	input.SendKeyEvent(KeyCode.UpArrow),		EventModifiers.Shift,	disregard);
		input.AddKeyHandler(KeyCode.DownArrow,	() =>	input.SendKeyEvent(KeyCode.DownArrow),		EventModifiers.Shift,	disregard);
		input.AddKeyHandler(KeyCode.LeftArrow,	() =>	input.SendKeyEvent(KeyCode.LeftArrow),		EventModifiers.Shift,	disregard);
		input.AddKeyHandler(KeyCode.RightArrow,	() =>	input.SendKeyEvent(KeyCode.RightArrow),		EventModifiers.Shift,	disregard);
	}
	#endregion

	#region Console
	protected void InitializeUI()
	{
		LogText.text = "";
		CommandInput.text = "";
	}

	public static void Show()
	{
		instance.Animator.Play("Show");
		instance.CommandInput.ActivateInputField();
		instance.CommandInput.Select();
		Observable.NextFrame().Subscribe(t => ScrollToBottom());
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
		Log(ObjectOrTableToString(obj));
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
	}

	public static string ObjectOrTableToString(object obj)
	{
		StringBuilder output = new StringBuilder();
		ObjectOrTableToString(output, obj, Depth, new List<object>());
		return output.ToString();
	}

	protected static void ObjectOrTableToString(StringBuilder output, object obj, int depth, List<object> traversed)
	{
		if (obj is LuaTable table)
		{
			bool first = true;
			output.Append("{");
			traversed.Add(table);
			table.ForEach<object, object>((key, value) => {
				if (first)
					first = false;
				else
					output.Append(", ");
				CyclicObjectOrTableToString(output, key, depth, traversed);
				output.Append(" = ");
				CyclicObjectOrTableToString(output, value, depth, traversed);
			});
			output.Append("}");
		}
		else
			Debugger.ObjectOrEnumerableToString(output, obj, false, NullString);
	}

	protected static void CyclicObjectOrTableToString(StringBuilder output, object obj, int depth, List<object> traversed)
	{
		if (obj is LuaTable)
		{
			if (traversed.Contains(obj))
				output.Append("*");
			else
			{
				if (depth > 1)
				{
					traversed.Add(obj);
					ObjectOrTableToString(output, obj, depth - 1, traversed);
				}
				else
					output.Append("...");
			}
		}
		else
			Debugger.ObjectOrEnumerableToString(output, obj, false, NullString);
	}

	public static void List(Type type)
	{
		MemberInfo[] members = type.GetMembers(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Instance);
		Log($"{type.FullName} : {type.BaseType.FullName}");
		members.ForEach(member => Log(MemberToString(type, member)));
	}

	protected static string MemberToString(Type type, MemberInfo member)
	{
		StringBuilder output = new StringBuilder();
		output.Append($"{type.FullName}.{member.Name}");
		switch (member.MemberType)
		{
			case MemberTypes.Field:
				{
					FieldInfo field = (FieldInfo) member;
					output.Append($" = {field.FieldType}");
					if (field.IsLiteral)
						output.Append(" (Read-only)");
					if (field.IsStatic)
						output.Append(" [Static]");
				}
				break;

			case MemberTypes.Property:
				{
					PropertyInfo property = (PropertyInfo) member;
					output.Append($" = {property.PropertyType}");
					if (!property.CanWrite)
						output.Append(" (Read-only)");
				}
				break;

			case MemberTypes.Method:
				{
					MethodInfo method = (MethodInfo) member;
					ParameterInfo[] parameters = method.GetParameters();
					output.Append("(");
					bool first = true;
					foreach (ParameterInfo parameterInfo in parameters)
					{
						if (first)
							first = false;
						else
							output.Append(", ");
						output.Append(parameterInfo.ParameterType.Name);
					}
					output.Append(")");
					if (method.IsStatic)
						output.Append(" [Static]");
				}
				break;
		}
		return output.ToString();
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
			Log(FormatCommand(command));
			AddToHistory(command);
			Execute(command);
			ClearCommand();
			Observable.NextFrame().Subscribe(t => ScrollToBottom());
		}
	}

	protected string FormatCommand(string command)
	{
		string[] lines = command.Split('\n');
		lines[0] = CommandPrefix + lines[0];
		string output = string.Join("\n  ", lines);
		return "<b>" + output + "</b>";
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
			results?.ForEach(result => Log(result));
		}
	}
	#endregion

	#region History
	protected List<string> history = new List<string>();
	protected int currentCommandIndex = 0;

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

#endif
}
