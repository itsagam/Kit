using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ConsoleInputField : InputField
{
	public List<(KeyCode key, EventModifiers modifiers, Action action)> KeyHandlers = new List<(KeyCode, EventModifiers, Action)>();

	public override void OnUpdateSelected(BaseEventData eventData)
	{
		Event e = new Event();
		bool consumedEvent = false;
		while (Event.PopEvent(e))
		{
			if (e.rawType == EventType.KeyDown)
			{
				consumedEvent = true;
				var (key, modifiers, action) = KeyHandlers.FirstOrDefault(t => t.key == e.keyCode && e.modifiers.HasFlag(t.modifiers));
				if (action != null)
					action();
				else
					KeyPressed(e);
			}
		}

		if (consumedEvent)
			UpdateLabel();
		eventData.Use();
	}

	public void AddKeyHandler(KeyCode key, Action action, EventModifiers modifiers = EventModifiers.None)
	{
		KeyHandlers.Add((key, modifiers, action));
	}
}