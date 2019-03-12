using UnityEngine.UI;

namespace Engine
{
	public static class SelectableExtensions
	{
		public static void SetInteractableImmediate(this Selectable selectable, bool value)
		{
			ColorBlock colorBlock = selectable.colors;
			float previousDuration = colorBlock.fadeDuration;
			colorBlock.fadeDuration = 0;
			selectable.colors = colorBlock;
			selectable.interactable = value;
			colorBlock.fadeDuration = previousDuration;
			selectable.colors = colorBlock;
		}
	}
}