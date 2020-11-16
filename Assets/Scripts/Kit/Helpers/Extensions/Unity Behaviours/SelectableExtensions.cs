using UnityEngine.UI;

namespace Kit
{
	/// <summary><see cref="Selectable" /> extensions.</summary>
	public static class SelectableExtensions
	{
		/// <summary>Changes the value of the <see cref="Selectable.interactable" /> property without triggering transitions.</summary>
		public static void SetInteractableImmediate(this Selectable selectable, bool value)
		{
			selectable.Disable();
			selectable.interactable = value;
			selectable.Enable();
		}
	}
}