using UnityEngine;

namespace Engine.UI.Buttons
{
	/// <summary>
	/// Button that switches to a specific step/screen in a <see cref="Wizard"/>.
	/// </summary>
	public class GotoButton: ButtonBehaviour
	{
		/// <summary>
		/// The wizard this button interacts with.
		/// </summary>
		[Tooltip("The wizard this button interacts with.")]
		public Wizard Wizard;

		/// <summary>
		/// The screen to switch to.
		/// </summary>
		[Tooltip("The screen to switch to.")]
		public Window Step;

		protected override void OnClick()
		{
			if (Wizard != null)
				Wizard.GoTo(Step);
		}
	}
}