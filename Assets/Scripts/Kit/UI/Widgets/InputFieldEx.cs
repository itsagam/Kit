using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kit.UI.Widgets
{
	/// <summary>
	/// An enhanced <see cref="InputField"/> that allows you to hook into key presses on or manually send key events to it.
	/// </summary>
	public class InputFieldEx: InputField
	{
		/// <summary>
		/// Defines a combination of keys and the action to take with it.
		/// </summary>
		public struct KeyHandler
		{
			/// <summary>
			/// The basic key to handle.
			/// </summary>
			public KeyCode Key;

			/// <summary>
			/// Key modifiers that should be pressed with it.
			/// </summary>
			public EventModifiers Modifiers;

			/// <summary>
			/// Key modifiers that should be dis-regarded.
			/// </summary>
			public EventModifiers Disregard;

			/// <summary>
			/// Method to call when the combination is pressed.
			/// </summary>
			public Action Action;
		}

		protected List<KeyHandler> keyHandlers = new List<KeyHandler>();

		/// <summary>
		/// Hook into a key combination.
		/// </summary>
		/// <param name="key">The basic key to handle.</param>
		/// <param name="action">Method to call when the combination is pressed.</param>
		/// <param name="modifiers">Key modifiers that should be pressed with it.</param>
		/// <param name="disregard">Key modifiers that should be dis-regarded.</param>
		public KeyHandler AddKeyHandler(KeyCode key,
										Action action,
										EventModifiers modifiers = EventModifiers.None,
										EventModifiers disregard = EventModifiers.None)
		{
			KeyHandler keyHandler = new KeyHandler
									{
										Key = key,
										Modifiers = modifiers,
										Disregard = disregard,
										Action = action
									};
			AddKeyHandler(keyHandler);
			return keyHandler;
		}

		/// <summary>
		/// Hook into a key combination.
		/// </summary>
		public void AddKeyHandler(KeyHandler keyHandler)
		{
			keyHandlers.Add(keyHandler);
		}

		/// <summary>
		/// Unhook a key combination.
		/// </summary>
		public void RemoveKeyHandler(KeyHandler keyHandler)
		{
			keyHandlers.Remove(keyHandler);
		}

		/// <summary>
		/// Manually send a key event to the input field.
		/// </summary>
		public virtual void SendKeyEvent(Event keyEvent)
		{
			KeyPressed(keyEvent);
		}

		/// <summary>
		/// Manually send a key event to the input field.
		/// </summary>
		public virtual void SendKeyEvent(KeyCode key, char character = default, EventModifiers modifiers = default)
		{
			Event keyEvent = new Event
							 {
								 type = EventType.KeyDown,
								 keyCode = key,
								 character = character,
								 modifiers = modifiers
							 };
			SendKeyEvent(keyEvent);
		}

		public override void OnUpdateSelected(BaseEventData eventData)
		{
			bool consumedEvent = false;
			Event e = new Event();
			while (Event.PopEvent(e))
				if (e.rawType == EventType.KeyDown)
				{
					consumedEvent = true;
					Action action = keyHandlers.FirstOrDefault(t => t.Key == e.keyCode && t.Modifiers == (e.modifiers & ~t.Disregard))
											   .Action;
					if (action != null)
					{
						action();
						break;
					}

					KeyPressed(e);
				}

			if (consumedEvent)
				UpdateLabel();
			eventData.Use();
		}
	}
}