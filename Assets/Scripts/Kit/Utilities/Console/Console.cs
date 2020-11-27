#if CONSOLE && (UNITY_EDITOR || DEVELOPMENT_BUILD)
#if TOUCHSCRIPT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Kit.UI.Widgets;
using TouchScript.Gestures;
using TouchScript.Layers;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using XLua;
using Object = UnityEngine.Object;

#endif

namespace Kit
{
	/// <summary>In-game Lua console. Press tilde (~) on PC or flick-down on mobile to show.</summary>
	public static class Console
	{
		/// <summary>Prefab location to the Console UI.</summary>
		public const string Prefab = "Console/Console";

		/// <summary>Maximum length of the log in characters.</summary>
		public static int Length = 10000;

		/// <summary>How deep to go when logging object contents.</summary>
		public static int Depth = 2;

		/// <summary>Garbage collector interval for the Lua environment of the Console.</summary>
		public const float GCInterval = 1.0f;

		/// <summary>Text to display for distinguishing typed content.</summary>
		public const string CommandPrefix = "> ";

		/// <summary>Text to display for <see langword="null" /> objects.</summary>
		public const string NullString = "nil";

		/// <summary><see cref="StringBuilder" /> for the entire log.</summary>
		public static readonly StringBuilder LogBuilder = new StringBuilder(Length);

		private static ConsoleUI instance;
		private static CompositeDisposable disposables = new CompositeDisposable();

		#region Initialization

		/// <summary>Initialize the Console.</summary>
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		public static void Initialize()
		{
			if (instance != null)
				return;

			if (CreateUI())
			{
				InitializeUI();
				RegisterLogging();
				RegisterInput();
				InitializeScripting();
			}
		}

		private static bool CreateUI()
		{
			ConsoleUI prefab = Resources.Load<ConsoleUI>(Prefab);
			if (prefab == null)
				return false;
			instance = Object.Instantiate(prefab);
			instance.name = prefab.name;
			Object.DontDestroyOnLoad(instance.gameObject);
			if (EventSystem.current == null)
				new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
			return true;
		}

		private static void RegisterInput()
		{
			if (Application.isMobilePlatform)
			{
#if TOUCHSCRIPT
				GameObject gameObject = instance.gameObject;

				FullscreenLayer layer = gameObject.AddComponent<FullscreenLayer>();
				layer.Type = FullscreenLayer.LayerType.Global;

				FlickGesture flick = gameObject.AddComponent<FlickGesture>();
				flick.Direction = FlickGesture.GestureDirection.Vertical;
				flick.Flicked += (o, e) =>
								 {
									 if (flick.ScreenFlickVector.y < 0 && !IsVisible)
										 Show();
									 else if (flick.ScreenFlickVector.y > 0 && IsVisible)
										 Hide();
								 };
#endif
			}
			else
				Observable
				   .EveryUpdate()
				   .Where(l => Input.GetKeyDown(KeyCode.BackQuote))
				   .Subscribe(l => Toggle())
				   .AddTo(disposables);

			const EventModifiers disregard = EventModifiers.FunctionKey | EventModifiers.Numeric | EventModifiers.CapsLock;
			InputFieldEx input = instance.CommandInput;
			input.AddKeyHandler(KeyCode.BackQuote,  () => { },                                      EventModifiers.None,  disregard);
			input.AddKeyHandler(KeyCode.Return,     Submit,                                         EventModifiers.None,  disregard);
			input.AddKeyHandler(KeyCode.Return,     () => input.SendKeyEvent(KeyCode.Return, '\n'), EventModifiers.Shift, disregard);
			input.AddKeyHandler(KeyCode.UpArrow,    SelectPreviousCommand,                          EventModifiers.None,  disregard);
			input.AddKeyHandler(KeyCode.DownArrow,  SelectNextCommand,                              EventModifiers.None,  disregard);
			input.AddKeyHandler(KeyCode.UpArrow,    () => input.SendKeyEvent(KeyCode.UpArrow),      EventModifiers.Shift, disregard);
			input.AddKeyHandler(KeyCode.DownArrow,  () => input.SendKeyEvent(KeyCode.DownArrow),    EventModifiers.Shift, disregard);
			input.AddKeyHandler(KeyCode.LeftArrow,  () => input.SendKeyEvent(KeyCode.LeftArrow),    EventModifiers.Shift, disregard);
			input.AddKeyHandler(KeyCode.RightArrow, () => input.SendKeyEvent(KeyCode.RightArrow),   EventModifiers.Shift, disregard);
		}

		#endregion

		#region Console

		private static void InitializeUI()
		{
			instance.LogText.text = "";
			instance.CommandInput.text = "";
		}

		/// <summary>Show the Console.</summary>
		public static void Show()
		{
			instance.Animator.Play("Show");
			instance.CommandInput.gameObject.SetActive(true);
			instance.CommandInput.ActivateInputField();
			instance.CommandInput.Select();
			Observable.NextFrame().Subscribe(t => ScrollToBottom());
		}

		/// <summary>Hide the Console.</summary>
		public static void Hide()
		{
			instance.Animator.Play("Hide");
			instance.CommandInput.DeactivateInputField();
			instance.CommandInput.gameObject.SetActive(false);
		}

		/// <summary>Show if not visible, and vice versa.</summary>
		public static void Toggle()
		{
			IsVisible = !IsVisible;
		}

		/// <summary>Get whether the Console is visible or show/hide it.</summary>
		public static bool IsVisible
		{
			get
			{
				AnimatorStateInfo state = instance.Animator.GetCurrentAnimatorStateInfo(0);
				if (state.IsName("Show"))
					return state.normalizedTime > 1;
				if (state.IsName("Hide"))
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

		private static string logEnd = Environment.NewLine;

		private static void RegisterLogging()
		{
			Application.logMessageReceived += OnLog;
		}

		private static void UnregisterLogging()
		{
			Application.logMessageReceived -= OnLog;
		}

		private static void OnLog(string message, string stackTrace, LogType type)
		{
			Log($"<i>{message}</i>");
		}

		/// <summary>Log an object on the Console.</summary>
		public static void Log(object obj)
		{
			if (instance == null)
				return;

			ObjectOrTableToString(LogBuilder, obj, Depth, new List<object>());
			TrimLog(LogBuilder.Length);
			LogBuilder.AppendLine();
			instance.LogText.text = LogBuilder.ToString();
		}

		/// <summary>Log a line on the Console.</summary>
		public static void Log(string line)
		{
			if (instance == null)
				return;

			TrimLog(LogBuilder.Length + line.Length);
			LogBuilder.AppendLine(line);
			instance.LogText.text = LogBuilder.ToString();
		}

		private static void TrimLog(int newLength)
		{
			if (newLength <= Length)
				return;

			int removeLength = newLength - Length;
			removeLength = LogBuilder.IndexOf(logEnd, removeLength) + logEnd.Length;
			LogBuilder.Remove(0, removeLength);
		}

		private static void ObjectOrTableToString(StringBuilder output, object obj, int depth, List<object> traversed)
		{
			if (obj is LuaTable table)
			{
				bool first = true;
				output.Append("{");
				traversed.Add(table);
				table.ForEach<object, object>((key, value) =>
											  {
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

		private static void CyclicObjectOrTableToString(StringBuilder output, object obj, int depth, List<object> traversed)
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

		/// <summary>List all members of a class on the Console.</summary>
		public static void List(Type type)
		{
			var members = type.GetMembers(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Instance);
			var extensions = from t in type.Assembly.GetTypes().Union(Assembly.GetExecutingAssembly().GetTypes())
							 //from t in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
							 where t.IsSealed && !t.IsGenericType && !t.IsNested
							 from method in t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
							 where method.IsDefined(typeof(ExtensionAttribute), false)
							 where method.GetParameters()[0].ParameterType == type
							 select method;

			var allMembers = members.Select(member => MemberToString(type, member))
									.Union(extensions.Select(member => ExtensionToString(type, member)))
									.OrderBy(member => member);

			Log("<b>" + type.FullName + (type.BaseType != typeof(object) ? ": " + type.BaseType.FullName : "") + "</b>");
			allMembers.ForEach(Log);
		}

		private static string MemberToString(Type type, MemberInfo member)
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
					var parameters = method.GetParameters();
					output.Append("(");
					parameters.Select(p => p.ParameterType.Name).Join(output);
					output.Append(")");
					if (method.IsStatic)
						output.Append(" [Static]");
				}
					break;
			}

			return output.ToString();
		}

		private static string ExtensionToString(Type type, MethodInfo method)
		{
			StringBuilder output = new StringBuilder();
			output.Append($"{type.FullName}.{method.Name}");

			var parameters = method.GetParameters();
			output.Append("(");
			parameters.Select(p => p.ParameterType.Name).Join(output);
			output.Append(")");

			return output.ToString();
		}

		/// <summary>Scroll the Console log to the bottom.</summary>
		public static void ScrollToBottom()
		{
			instance.LogScroll.verticalNormalizedPosition = 0;
		}

		/// <summary>Scroll the Console log to the top.</summary>
		public static void ScrollToTop()
		{
			instance.LogScroll.verticalNormalizedPosition = 1;
		}

		/// <summary>Clear the Console log.</summary>
		public static void ClearLog()
		{
			LogBuilder.Clear();
			instance.LogText.text = "";
		}

		#endregion

		#region Command

		private static void Submit()
		{
			string command = instance.CommandInput.text;
			if (command == "")
				return;

			Log(FormatCommand(command));
			AddToHistory(command);
			Execute(command);
			ClearCommand();
			Observable.NextFrame().Subscribe(t => ScrollToBottom());
		}

		private static string FormatCommand(string command)
		{
			var lines = command.Split('\n');
			lines[0] = CommandPrefix + lines[0];
			string output = string.Join("\n  ", lines);
			return "<b>" + output + "</b>";
		}

		/// <summary>Clear the command input field.</summary>
		public static void ClearCommand()
		{
			instance.CommandInput.ActivateInputField();
			instance.CommandInput.Select();
			instance.CommandInput.text = "";
		}

		#endregion

		#region Execution

		private static LuaEnv scriptEnv;

		private static void InitializeScripting()
		{
			scriptEnv = new LuaEnv();
			scriptEnv.DoString("require 'Lua/General'");
			scriptEnv.DoString("require 'Lua/Console'");
			Observable.Timer(TimeSpan.FromSeconds(GCInterval)).Subscribe(l => scriptEnv.Tick()).AddTo(disposables);
		}

		/// <summary>Execute a Lua command or expression on the Console.</summary>
		/// <param name="command">Command or expression to execute.</param>
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
				var results = scriptEnv.DoString(commandActual);
				results?.ForEach(Log);
			}
		}

		#endregion

		#region History

		private static List<string> history = new List<string>();
		private static int currentCommandIndex = 0;

		private static void AddToHistory(string command)
		{
			history.Add(command);
			currentCommandIndex = history.Count;
		}

		private static void SelectPreviousCommand()
		{
			SelectCommand(currentCommandIndex - 1);
		}

		private static void SelectNextCommand()
		{
			SelectCommand(currentCommandIndex + 1);
		}

		private static void SelectCommand(int index)
		{
			InputFieldEx input = instance.CommandInput;
			if (index >= history.Count)
			{
				input.text = "";
				currentCommandIndex = history.Count;
			}
			else if (index < 0)
			{
			}
			else
			{
				input.text = history[index];
				currentCommandIndex = index;
			}

			input.MoveTextEnd(false);
		}

		/// <summary>Clear Console history.</summary>
		public static void ClearHistory()
		{
			history.Clear();
		}

		#endregion

		#region Destruction

		/// <summary>Destroy the Console.</summary>
		public static void Destroy()
		{
			disposables.Dispose();
			scriptEnv.Dispose();
			UnregisterLogging();
			instance.gameObject.Destroy();
			instance = null;
		}

		#endregion
	}
}
#endif