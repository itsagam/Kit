using System;
using Kit.UI.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace Kit
{
	/// <summary>UI hookup for the <see cref="Console" />.</summary>
	public class ConsoleUI: MonoBehaviour
	{
		/// <summary><see cref="UnityEngine.Animator" /> to use for show/hide animations.</summary>
		public Animator Animator;

		/// <summary><see cref="ScrollRect" /> for the console log.</summary>
		public ScrollRect LogScroll;

		/// <summary>The text-box for the console log.</summary>
		public Text LogText;

		/// <summary>The command input-field.</summary>
		public InputFieldEx CommandInput;

#if CONSOLE
		private void OnDestroy()
		{
			Console.Destroy();
		}
#endif
	}
}