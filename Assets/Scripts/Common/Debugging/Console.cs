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

// TODO: Set MonoBehavior variables
// TODO: Call MonoBehavior functions
// TODO: Command history
// TODO: Command validation
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

	public Canvas Canvas;
	public ScrollRect LogScroll;
	public Text LogText;
	public InputField CommandInput;

	protected static Console instance = null;
	protected StringBuilder log = new StringBuilder(Length + 1);

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
		RegisterGesture();
		SetupUI();
		DontDestroyOnLoad(gameObject);
	}

	public void RegisterLogging()
	{
		Application.logMessageReceived += OnLog;
	}

	public void UnregisterLogging()
	{
		Application.logMessageReceived -= OnLog;
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
		Log($"<color={LogColor}>{message}</color>");
	}

	protected void SetupUI()
	{
		Canvas.gameObject.SetActive(false);
		LogScroll.transform.localScale = new Vector3(1, 0, 1);
		LogText.text = "";
		CommandInput.text = "";
		CommandInput.onValidateInput += OnValidateInput;
	}

	protected char OnValidateInput(string text, int charIndex, char addedChar)
	{
		if (addedChar == '\n')
		{
			OnSubmit();
			return '\0';
		}
		else
			return addedChar;
	}

	protected void OnSubmit()
	{
		string command = CommandInput.text;
		if (command != "")
		{
			Log(CommandPrefix + command);
			Execute(command);
			ClearCommand();
		}
	}
	#endregion

	#region Public methods
	public void Show()
	{
		Canvas.gameObject.SetActive(true);
		LogScroll.transform.DOScaleY(1.0f, TransitionTime).SetEase(Ease.InSine);
		CommandInput.ActivateInputField();
		CommandInput.Select();
		//Observable.NextFrame().Subscribe(t => CommandInput.MoveTextEnd(true));
	}

	public void Hide()
	{
		LogScroll.transform.DOScaleY(0.0f, TransitionTime).SetEase(Ease.OutSine).OnComplete(() =>
		{
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

	public void Log(object obj)
	{
		Log(Debugger.ObjectOrEnumerableToString(obj, true));
	}

	public void Log(string line)
	{
		int postLength = log.Length + line.Length;
		if (postLength > Length)
			log.Remove(0, postLength - Length);
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

	public void ClearCommand()
	{
		CommandInput.ActivateInputField();
		CommandInput.Select();
		CommandInput.text = "";
	}
#endregion

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
			{
				foreach (MemberInfo member in allMembers)
					Log(MemberToString(member));
			}
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

	protected void OnDestroy()
	{
		UnregisterLogging();
	}
}