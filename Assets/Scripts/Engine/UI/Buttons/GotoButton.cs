namespace Engine.UI.Buttons
{
	/// <summary>
	/// Button that goes to a specific step/screen in a <see cref="Wizard"/>.
	/// </summary>
	public class GotoButton: ButtonBehaviour
	{
		public Wizard Wizard;
		public Window Step;

		protected override void OnClick()
		{
			if (Wizard != null)
				Wizard.GoTo(Step);
		}
	}
}