using UnityEngine.UI;

namespace Engine
{
	public static class SelectableExtensions
	{
		/// <summary>
		/// Changes the value of the interactable property without triggering transitions.
		/// </summary>
		public static void SetInteractableImmediate(this Selectable selectable, bool value)
		{
			selectable.Disable();
			selectable.interactable = value;
			selectable.Enable();
		}
	}
}