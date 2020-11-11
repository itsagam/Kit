using UnityEngine;

namespace Engine.UI.Layout
{
	/// <summary>
	/// Give a UI element the size of this <see cref="rectTransform"/> whenever it's resized.
	/// </summary>
	public class MatchSizeLayout: MonoBehaviour
	{
		public RectTransform Element;
		public Vector2 Padding;

		public bool Width = true;
		public bool Height = true;

		protected RectTransform rectTransform;

		protected void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
		}

		protected void OnRectTransformDimensionsChange()
		{
			if (!Element || !rectTransform)
				return;

			Vector2 newSize = Element.sizeDelta;
			if (Width)
				newSize.x = rectTransform.sizeDelta.x + Padding.x;
			if (Height)
				newSize.y = rectTransform.sizeDelta.y + Padding.y;
			Element.sizeDelta = newSize;
		}
	}
}