using UnityEngine.UI;

namespace Engine
{
	public static class SelectableExtensions
	{
		public static void SetInteractableImmediate(this Selectable selectable, bool value)
		{
			selectable.Disable();
			selectable.interactable = value;
			selectable.Enable();
		}
	}
}