using UnityEngine;
using UnityEngine.UI;

namespace Engine.UI.Behaviours
{
	// Allows to set on/off sprite of a Toggle
	[RequireComponent(typeof(Toggle))]
	public class ToggleSprite: ToggleBehaviour
	{
		public Sprite OnSprite;
		public Sprite OffSprite;

		protected override void SetValue(bool value)
		{
			Image image = toggle.targetGraphic as Image;
			if (image == null)
				return;
			image.sprite = value ? OnSprite : OffSprite;
		}
	}
}