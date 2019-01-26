﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InputFieldEx : InputField
{
	protected List<(KeyCode key, EventModifiers modifiers, EventModifiers disregard, Action action)> KeyHandlers = new List<(KeyCode, EventModifiers, EventModifiers, Action)>();

	public void AddKeyHandler(KeyCode key, Action action, EventModifiers modifiers = EventModifiers.None, EventModifiers disregard = EventModifiers.None)
	{
		KeyHandlers.Add((key, modifiers, disregard, action));
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
				var action = KeyHandlers.FirstOrDefault(t => t.key == e.keyCode && t.modifiers == (e.modifiers & ~t.disregard)).action;
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

	public virtual void SendKeyEvent(Event e)
	{
		KeyPressed(e);
	}
	
	public virtual void SendKeyEvent(KeyCode key, char character = default, EventModifiers modifiers = default)
	{
		Event keyEvent = new Event {
			type = EventType.KeyDown,
			keyCode = key,
			character = character,
			modifiers = modifiers,
		};
		SendKeyEvent(keyEvent);
	}
}