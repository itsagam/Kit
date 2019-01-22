using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CustomInputField : InputField
{
	protected List<(KeyCode key, EventModifiers modifiers, Action action)> KeyHandlers = new List<(KeyCode, EventModifiers, Action)>();

	public override void OnUpdateSelected(BaseEventData eventData)
	{
		Event e = new Event();
		while (Event.PopEvent(e))
		{
			if (e.rawType == EventType.KeyDown)
			{
				var (key, modifiers, action) = KeyHandlers.FirstOrDefault(t => t.key == e.keyCode && e.modifiers.HasFlag(t.modifiers));
				if (action != null)
					action();
				else
					KeyPressed(e);
			}
			UpdateLabel();
		}	
		eventData.Use();
	}

	public void AddKeyHandler(KeyCode key, Action action, EventModifiers modifiers = EventModifiers.None)
	{
		KeyHandlers.Add((key, modifiers, action));
	}
}