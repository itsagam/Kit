using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InputFieldEx : InputField
{
	public struct KeyHandler
	{
		public KeyCode Key;
		public EventModifiers Modifiers;
		public EventModifiers Disregard;
		public Action Action;
	}

	protected List<KeyHandler> keyHandlers = new List<KeyHandler>();

	public KeyHandler AddKeyHandler(KeyCode key, Action action, EventModifiers modifiers = EventModifiers.None, EventModifiers disregard = EventModifiers.None)
	{
		KeyHandler keyHandler = new KeyHandler()
		{
			Key = key,
			Modifiers = modifiers,
			Disregard = disregard,
			Action = action
		};
		AddKeyHandler(keyHandler);
		return keyHandler;
	}

	public void AddKeyHandler(KeyHandler keyHandler)
	{
		keyHandlers.Add(keyHandler);
	}

	public void RemoveKeyHandler(KeyHandler keyHandler)
	{
		keyHandlers.Remove(keyHandler);
	}

	public virtual void SendKeyEvent(Event e)
	{
		KeyPressed(e);
	}

	public virtual void SendKeyEvent(KeyCode key, char character = default, EventModifiers modifiers = default)
	{
		Event keyEvent = new Event
		{
			type = EventType.KeyDown,
			keyCode = key,
			character = character,
			modifiers = modifiers,
		};
		SendKeyEvent(keyEvent);
	}

	public override void OnUpdateSelected(BaseEventData eventData)
	{
		bool consumedEvent = false;
		Event e = new Event();
		while (Event.PopEvent(e))
		{
			if (e.rawType == EventType.KeyDown)
			{
				consumedEvent = true;
				var action = keyHandlers.FirstOrDefault(t => t.Key == e.keyCode && t.Modifiers == (e.modifiers & ~t.Disregard)).Action;
				if (action != null)
				{
					action();
					break;
				}
				else
					KeyPressed(e);
			}	
		}
		if (consumedEvent)
			UpdateLabel();
		eventData.Use();
	}
}