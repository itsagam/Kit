﻿using Kit.UI.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace Kit
{
	/// <summary>
	/// UI hookup for the <see cref="Console"/>.
	/// </summary>
	public class ConsoleUI: MonoBehaviour
	{
		public Animator Animator;
		public ScrollRect LogScroll;
		public Text LogText;
		public InputFieldEx CommandInput;
	}
}