using Kit.UI.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace Kit
{
	/// <summary>UI hookup for the <see cref="Console" />.</summary>
	public class ConsoleUI: MonoBehaviour
	{
		/// <summary>The <see cref="Animator"/> to use for show/hide animations.</summary>
		public Animator Animator;

		/// <summary>The <see cref="ScrollRect"/> for the console log.</summary>
		public ScrollRect LogScroll;

		/// <summary>The text-box for the console log.</summary>
		public Text LogText;

		/// <summary>The command input-field.</summary>
		public InputFieldEx CommandInput;
	}
}