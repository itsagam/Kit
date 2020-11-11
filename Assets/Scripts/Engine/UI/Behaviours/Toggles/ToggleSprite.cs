using UnityEngine;
using UnityEngine.UI;

namespace Engine.UI.Behaviours
{
	/// <summary>
	/// Allows to set the on/off sprite of a <see cref="UnityEngine.UI.Toggle" />.
	/// </summary>
	[RequireComponent(typeof(Toggle))]
	public class ToggleSprite: ToggleBehaviour
	{
		public Sprite OnSprite;
		public Sprite OffSprite;

		protected override void OnValueChanged(bool value)
		{
			Image image = toggle.targetGraphic as Image;
			if (image == null)
				return;
			image.sprite = value ? OnSprite : OffSprite;
		}
	}
}