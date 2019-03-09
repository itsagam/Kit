using UnityEngine.UI;

public static class SelectableExtensions
{
	public static void SetInteractableImmediate(this Selectable selectable, bool value)
	{
		var colorBlock = selectable.colors;
		var previousDuration = colorBlock.fadeDuration;
		colorBlock.fadeDuration = 0;
		selectable.colors = colorBlock;
		selectable.interactable = value;
		colorBlock.fadeDuration = previousDuration;
		selectable.colors = colorBlock;
	}
}