using UnityEngine;
using UnityEngine.UI;

namespace Kit.UI.Behaviours
{
	/// <summary>Allows to set the on/off sprite of a <see cref="UnityEngine.UI.Toggle" />.</summary>
	[RequireComponent(typeof(Toggle))]
	public class ToggleSprite: ToggleBehaviour
	{
		/// <summary><see cref="Sprite" /> to use for the On state.</summary>
		[Tooltip("Sprite to use for the On state.")]
		public Sprite OnSprite;

		/// <summary><see cref="Sprite" /> to use for the Off state.</summary>
		[Tooltip("Sprite to use for the Off state.")]
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