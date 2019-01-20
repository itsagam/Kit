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
using DG.Tweening;
using UniRx;
using XLua;

// TODO: Provide a way to list all members
// TODO: Hook print function to Console
// TODO: Find a way to remove "CS."

// TODO: Make public methods static

// TODO: Use swipe-down gesture
// TODO: Provide multiple-line support
// TODO: Autocomplete objects
// TODO: Autocomplete functions/variables
// TODO: Autocomplete parameters

public class Console : MonoBehaviour
{
	public const bool Enabled = true;
	public const string Prefab = "Console/Console";
	public const int Length = 5000;
	public const double GestureTime = 250;
	public const float GestureDistance = 0.05f;
	public const float TransitionTime = 0.3f;
	public const string LogColor = "#00DDFF";
	public const string CommandPrefix = "> ";
	public const string NullString = "nil";

	public Canvas Canvas;
	public ScrollRect LogScroll;
	public Text LogText;
	public ConsoleInputField CommandInput;

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
		RegisterGesture();
		InitializeLua();
		InitializeUI();
		InitializeHistory();
		DontDestroyOnLoad(gameObject);
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
			.Subscribe(b => Toggle())
			.AddTo(this);
	}

	protected void RegisterInput()
	{
		CommandInput.AddKeyHandler(KeyCode.Return, Submit);
		CommandInput.AddKeyHandler(KeyCode.UpArrow, SelectPreviousCommand);
		CommandInput.AddKeyHandler(KeyCode.DownArrow, SelectNextCommand);
	}

	protected void InitializeUI()
	{
		Canvas.gameObject.SetActive(false);
		LogScroll.transform.localScale = new Vector3(1, 0, 1);
		LogText.text = "";
		CommandInput.text = "";
	}
	#endregion

	#region Console
	public void Show()
	{
		Canvas.gameObject.SetActive(true);
		LogScroll.transform.DOScaleY(1.0f, TransitionTime).SetEase(Ease.InSine);
		CommandInput.ActivateInputField();
		CommandInput.Select();
	}

	public void Hide()
	{
		LogScroll.transform.DOScaleY(0.0f, TransitionTime).SetEase(Ease.OutSine).OnComplete(() => {
			Canvas.gameObject.SetActive(false);
		});
	}

	public void Toggle()
	{
		IsVisible = !IsVisible;
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
		string luaLibrary = ResourceManager.ReadText(ResourceFolder.StreamingAssets, "Lua/LuaLibrary.lua");
		luaEnv.DoString(luaLibrary);
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

/*
public void Execute(string command)
{
	string[] sides = command.SplitAndTrim('=');
	if (sides.Length > 2)
	{
		Log("There cannot be more than one \"=\" operator.");
		return;
	}

	string lhs = sides[0];
	string rhs = sides.Length == 2 ? sides[1] : null;

	string memberPath;
	string[] argStrings;
	int openIndex = lhs.IndexOf('(');
	if (openIndex >= 0)
	{
		int closeIndex = lhs.LastIndexOf(')');
		if (closeIndex < 0)
		{
			Log("Closing parenthesis not found.");
			return;
		}
		memberPath = lhs.Substring(0, openIndex).Trim();
		string argsString = lhs.Slice(openIndex + 1, closeIndex).Trim();
		if (argsString.IsNullOrEmpty())
			argStrings = new string[0];
		else
			argStrings = argsString.SplitAndTrim(',');
	}
	else
	{
		memberPath = lhs;
		argStrings = null;
	}

	if (argStrings != null && rhs != null)
	{
		Log("You cannot try to call a method and set a value at the same time.");
		return;
	}

	string typeName;
	string memberName;
	int lastDotIndex = memberPath.LastIndexOf('.');
	if (lastDotIndex >= 0)
	{
		typeName = memberPath.Substring(0, lastDotIndex).Trim();
		memberName = memberPath.Substring(lastDotIndex + 1).Trim();
	}
	else
	{
		typeName = memberPath;
		memberName = null;
	}

	Execute(typeName, memberName, rhs, argStrings);
}


protected void Execute(string typeName, string memberName, string valueString, string[] argStrings)
{
	Assembly assembly = Assembly.GetExecutingAssembly();
	Type type = assembly.GetType(typeName, false, true);
	if (type == null)
	{
		Log($"Class \"{typeName}\" was not found.");
		return;
	}

	// No member specified, list all accessible members and return
	if (memberName == null)
	{
		MemberInfo[] allMembers = type.GetMembers(BindingFlags.Public | BindingFlags.Static);
		if (allMembers.Length > 0)
			allMembers.ForEach(m => Log(MemberToString(m)));
		else
			Log($"Class \"{typeName}\" has no accessible members.");
		return;
	}

	MemberTypes types = default;
	if (valueString != null)
		types = MemberTypes.Field | MemberTypes.Property;
	else if (argStrings != null)
		types = MemberTypes.Method;
	else
	{
		types = MemberTypes.Field | MemberTypes.Property | MemberTypes.Method;
		argStrings = new string[0];
	}

	MemberInfo[] members = type.GetMember(memberName, types, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
	if (members.Length == 0)
	{
		Log($"Member \"{memberName}\" was not found in the class.");
		return;
	}

	foreach (MemberInfo member in members)
		switch (member.MemberType)
		{
			case MemberTypes.Field:
				try
				{
					FieldInfo field = (FieldInfo) member;
					if (valueString != null)
					{
						object value = TypeDescriptor.GetConverter(field.FieldType).ConvertFromString(valueString);
						field.SetValue(null, value);
					}
					Log(field.GetValue(null));
				}
				catch (Exception e)
				{
					Log(e.Message);
				}
				return;

			case MemberTypes.Property:
				try
				{
					PropertyInfo property = (PropertyInfo) member;
					if (valueString != null)
					{
						object value = TypeDescriptor.GetConverter(property.PropertyType).ConvertFromString(valueString);
						property.SetValue(null, value);
					}
					Log(property.GetValue(null));
				}
				catch (Exception e)
				{
					Log(e.Message);
				}
				return;

			case MemberTypes.Method:
				try
				{
					MethodInfo method = (MethodInfo) member;
					ParameterInfo[] parameters = method.GetParameters();
					if (parameters.Length == argStrings.Length)
					{
						object[] arguments = new object[parameters.Length];
						for (int i = 0; i < parameters.Length; i++)
							arguments[i] = TypeDescriptor.GetConverter(parameters[i].ParameterType).ConvertFromString(argStrings[i]);							
						object result = method.Invoke(null, arguments);
						if (method.ReturnType != typeof(void))
							Log(result.ToString());
						return;
					}
				}
				catch (Exception)
				{
				}
				break;
		}
	Log("No member matching given parameters was found.");
}

protected string MemberToString(MemberInfo member)
{
	string output = $"{member.ReflectedType.FullName}.{member.Name}";
	switch (member.MemberType)
	{
		case MemberTypes.Field:
			{
				FieldInfo field = (FieldInfo) member;
				output += $" = {field.FieldType}";
				if (field.IsLiteral)
					output += " (Read-only)";
			}
			break;

		case MemberTypes.Property:
			{
				PropertyInfo property = (PropertyInfo) member;
				output += $" = {property.PropertyType}";
				if (!property.CanWrite)
					output += " (Read-only)";
			}
			break;

		case MemberTypes.Method:
			{
				MethodInfo method = (MethodInfo) member;
				ParameterInfo[] parameters = method.GetParameters();
				output += "(";
				bool first = true;
				foreach (ParameterInfo parameterInfo in parameters)
				{
					if (!first)
						output += ", ";
					output += parameterInfo.ParameterType.Name;
					first = false;
				}
				output += ")";
			}
			break;
	}
	return output;
}
*/
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